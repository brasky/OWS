using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OWSData.Models.StoredProcs;
using OWSData.Models.Tables;
using OWSData.Repositories.Interfaces;
using OWSShared.Interfaces;
using OWSShared.Messages;
using OWSShared.Options;
using RabbitMQ.Client;
using OWS.Interfaces;
using Microsoft.AspNetCore.Mvc;
using OWSData.Models.Composites;
using Orleans.Runtime;
using System.Text.RegularExpressions;

namespace OWS.Grains
{
    public sealed class CharacterGrain : BaseGrain, ICharacterGrain
    {
        private readonly ILogger<CharacterGrain> _logger;
        private readonly IUsersRepository _usersRepository;
        private readonly ICharactersRepository _charactersRepository;
        private readonly IOptions<RabbitMQOptions> _rabbitMQOptions;
        private readonly ICustomCharacterDataSelector _customCharacterDataSelector;
        private readonly IGetReadOnlyPublicCharacterData _getReadOnlyPublicCharacterData;

        public CharacterGrain(

            ILogger<CharacterGrain> logger,
            IUsersRepository usersRepository,
            ICharactersRepository charactersRepository,
            IOptions<RabbitMQOptions> rabbitMQOptions,
            ICustomCharacterDataSelector customCharacterDataSelector,
            IGetReadOnlyPublicCharacterData getReadOnlyPublicCharacterData)
        {
            _logger = logger;
            _usersRepository = usersRepository;
            _charactersRepository = charactersRepository;
            _rabbitMQOptions = rabbitMQOptions;
            _customCharacterDataSelector = customCharacterDataSelector;
            _getReadOnlyPublicCharacterData = getReadOnlyPublicCharacterData;
        }

        private string ValidateCharacterName()
        {
            var charName = this.GetPrimaryKeyString();
            string errorMessage = string.Empty;
            //Test for empty Character Names or Character Names that are shorter than the minimum Character name Length
            if (String.IsNullOrEmpty(charName) || charName.Length < 4)
            {
                return "Please enter a valid Character Name that is at least 4 characters in length.";
            }

            Regex regex = new Regex(@"^\w+$");
            //Test for Character Names that use characters other than Letters (uppercase and lowercase) and Numbers.
            if (!regex.IsMatch(charName))
            {
                return "Please enter a Character Name that only contains letters and numbers.";
            }

            return string.Empty;
        }

        public async Task<CreateCharacter> Create(Guid userSessionId, string className)
        {
            Guid customerGuid = GetCustomerId();

            var errorMessage = ValidateCharacterName();
            if (!String.IsNullOrEmpty(errorMessage))
            {
                CreateCharacter createCharacterErrorMessage = new CreateCharacter();
                createCharacterErrorMessage.ErrorMessage = errorMessage;
                return createCharacterErrorMessage;
            }

            var output = await _usersRepository.CreateCharacter(customerGuid, userSessionId, this.GetPrimaryKeyString(), className);

            return output;
        }

        public async Task<SuccessAndErrorMessage> CreateUsingDefaultCharacterValues(Guid userSessionId, string defaultSetName)
        {
            Guid customerGuid = GetCustomerId();

            //Validate Character Name
            string errorMessage = ValidateCharacterName();
            if (!String.IsNullOrEmpty(errorMessage))
            {
                SuccessAndErrorMessage successAndErrorMessage = new SuccessAndErrorMessage();
                successAndErrorMessage.Success = false;
                successAndErrorMessage.ErrorMessage = errorMessage;
                return successAndErrorMessage;
            }

            var charName = this.GetPrimaryKeyString();
            //Make sure Character Name is Unique
            var characterToLookup = await _charactersRepository.GetCharByCharName(customerGuid, charName);

            if (characterToLookup != null && characterToLookup.UserGuid != Guid.Empty)
            {
                SuccessAndErrorMessage successAndErrorMessage = new SuccessAndErrorMessage();
                successAndErrorMessage.Success = false;
                successAndErrorMessage.ErrorMessage = "Character name already exists!";
                return successAndErrorMessage;
            }

            //Get User Session
            var userSession = await _usersRepository.GetUserSession(customerGuid, userSessionId);

            if (userSession == null || !userSession.UserGuid.HasValue)
            {
                SuccessAndErrorMessage successAndErrorMessage = new SuccessAndErrorMessage();
                successAndErrorMessage.Success = false;
                successAndErrorMessage.ErrorMessage = "Invalid User Session";
                return successAndErrorMessage;
            }

            await _usersRepository.CreateCharacterUsingDefaultCharacterValues(customerGuid, userSession.UserGuid.Value, charName,
                defaultSetName);

            return new SuccessAndErrorMessage() { Success = true, ErrorMessage = "" };
        }

        public async Task<CharacterAndCustomData> PublicGetByNameRequest(Guid userSessionId)
        {
            var customerGuid = GetCustomerId();

            CharacterAndCustomData Output = new CharacterAndCustomData();

            var characterName = this.GetPrimaryKeyString();

            //Get the User Session
            GetUserSession userSession = await _usersRepository.GetUserSession(customerGuid, userSessionId);

            //Make sure the User Session is valid
            if (userSession == null || !userSession.UserGuid.HasValue)
            {
                //TODO: rework to be able to return success bool / error msg instead of throwing
                throw new ArgumentException("User session is not valid");
            }

            //Get character data
            GetCharByCharName characterData = await _charactersRepository.GetCharByCharName(customerGuid, characterName);

            //Make sure the character data is valid and in the right User Session
            if (characterData == null || !characterData.UserGuid.HasValue || characterData.UserGuid != userSession.UserGuid)
            {
                return Output;
            }

            //Assign the character data to the output object
            Output.CharacterData = characterData;

            //Get custom character data
            IEnumerable<CustomCharacterData> customCharacterDataItems = await _charactersRepository.GetCustomCharacterData(customerGuid, characterName);

            //Initialize the list
            Output.CustomCharacterDataRows = new List<CustomCharacterDataDTO>();

            //Loop through all the CustomCharacterData rows
            foreach (CustomCharacterData currentCustomCharacterData in customCharacterDataItems)
            {
                //Use the ICustomCharacterDataSelector implementation to filter which fields are returned
                if (_customCharacterDataSelector.ShouldExportThisCustomCharacterDataField(currentCustomCharacterData.CustomFieldName))
                {
                    CustomCharacterDataDTO customCharacterDataDTO = new CustomCharacterDataDTO()
                    {
                        CustomFieldName = currentCustomCharacterData.CustomFieldName,
                        FieldValue = currentCustomCharacterData.FieldValue
                    };

                    //Add the filtered Custom Character Data
                    Output.CustomCharacterDataRows.Add(customCharacterDataDTO);
                }
            }

            //Add Read-only Character Data
            CustomCharacterDataDTO readOnlyCharacterData = new CustomCharacterDataDTO()
            {
                CustomFieldName = "ReadOnlyCharacterData",
                FieldValue = await _getReadOnlyPublicCharacterData.GetReadOnlyPublicCharacterData(characterData.CharacterId)
            };
            Output.CustomCharacterDataRows.Add(readOnlyCharacterData);

            return Output;
        }

        public async Task<JoinMapByCharName> GetServerToConnectTo(string zoneName, int playerGroupType)
        {
            var Output = new JoinMapByCharName();

            Guid customerGuid = GetCustomerId();

            //If ZoneName is empty, look it up from the character.  This is used for the inital login.
            if (String.IsNullOrEmpty(zoneName) || zoneName == "GETLASTZONENAME")
            {
                GetCharByCharName character = await _charactersRepository.GetCharByCharName(customerGuid, this.GetPrimaryKeyString());

                //If we can't find the character by name, then return BadRequest.
                if (character == null)
                {
                    Output.Success = false;
                    Output.ErrorMessage = $"Character with name {this.GetPrimaryKeyString()}";
                    return Output;
                }

                zoneName = character.MapName;
            }

            //If the ZoneName is empty, return an error
            if (String.IsNullOrEmpty(zoneName))
            {
                Output.Success = false;
                Output.ErrorMessage = "GetServerToConnectTo: ZoneName is NULL or Empty.  Make sure the character is assigned to a Zone!";
                return Output;
            }

            JoinMapByCharName joinMapByCharacterName = await _charactersRepository.JoinMapByCharName(
                customerGUID: customerGuid,
                characterName: this.GetPrimaryKeyString(),
                zoneName: zoneName,
                playerGroupType: playerGroupType);

            bool readyForPlayersToConnect = false;

            if (joinMapByCharacterName == null || joinMapByCharacterName.WorldServerID < 1)
            {
                Output.Success = false;
                Output.ErrorMessage = "GetServerToConnectTo: WorldServerID is less than 1.  Make sure you setup at least one valid World Server and that it is currently running!";
                return Output;
            }

            //There is no zone server running that will accept our connection, so start up a new one
            if (joinMapByCharacterName.NeedToStartupMap)
            {
                ConnectionFactory factory = new()
                {
                    HostName = _rabbitMQOptions.Value.RabbitMQHostName,
                    Port = _rabbitMQOptions.Value.RabbitMQPort,
                    UserName = _rabbitMQOptions.Value.RabbitMQUserName,
                    Password = _rabbitMQOptions.Value.RabbitMQPassword
                };

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchange: "ows.serverspinup",
                            type: "direct",
                            durable: false,
                            autoDelete: false);

                        MQSpinUpServerMessage serverSpinUpMessage = new()
                        {
                            CustomerGUID = customerGuid,
                            WorldServerID = joinMapByCharacterName.WorldServerID,
                            ZoneInstanceID = joinMapByCharacterName.MapInstanceID,
                            MapName = joinMapByCharacterName.MapNameToStart,
                            Port = joinMapByCharacterName.Port
                        };

                        var body = serverSpinUpMessage.Serialize();
                        channel.BasicPublish(exchange: "ows.serverspinup",
                                             routingKey: String.Format("ows.serverspinup.{0}", joinMapByCharacterName.WorldServerID),
                                             basicProperties: null,
                                             body: body);
                    }
                }

                //TODO: it doesnt make any sense to delay before giving the client a server to connect to
                //If we have begun to spin up a server, we should just give the client the ip and let them try to connect
                //If it takes too long client-side then they should reach back out to the api to get a new server.
                //i.e. why make the client _and_ the api server wait when we could just make the client wait.

                //Wait OWSGeneralConfig.SecondsToWaitBeforeFirstPollForSpinUp seconds before the first CheckMapInstanceStatus to give it time to spin up
                //Thread.Sleep(owsGeneralConfig.Value.SecondsToWaitBeforeFirstPollForSpinUp);

                //readyForPlayersToConnect = await WaitForServerReadyToConnect(CustomerGUID, joinMapByCharacterName.MapInstanceID);
            }
            //We found a zone server we can connect to, but it is still spinning up.  Wait until it is ready to connect to (up to OWSGeneralConfig.SecondsToWaitForServerSpinUp seconds).
            else if (joinMapByCharacterName.MapInstanceID > 0 && joinMapByCharacterName.MapInstanceStatus == 1)
            {
                //see comment above for why this doesn't make sense.

                //CheckMapInstanceStatus every OWSGeneralConfig.SecondsToWaitInBetweenSpinUpPolling seconds for up to OWSGeneralConfig.SecondsToWaitForServerSpinUp seconds
                //readyForPlayersToConnect = await WaitForServerReadyToConnect(CustomerGUID, joinMapByCharacterName.MapInstanceID);
            }
            //We found a zone server we can connect to and it is ready to connect
            else if (joinMapByCharacterName.MapInstanceID > 0 && joinMapByCharacterName.MapInstanceStatus == 2)
            {
                //The zone server is ready to connect to
                readyForPlayersToConnect = true;
            }

            //The zone instance is ready, so connect the character to the map instance in our data store
            if (readyForPlayersToConnect)
            {
                await _charactersRepository.AddCharacterToMapInstanceByCharName(customerGuid, this.GetPrimaryKeyString(), joinMapByCharacterName.MapInstanceID);
            }

            Output = joinMapByCharacterName;
            Output.Success = true;
            Output.ErrorMessage = "";
            return Output;
        }

        public async Task AddOrUpdateCustomData(AddOrUpdateCustomCharacterData request)
        {
            Guid customerGuid = GetCustomerId();

            await _charactersRepository.AddOrUpdateCustomCharacterData(customerGuid, request);
        }

        public async Task<GetCharByCharName> GetByName()
        {
            Guid customerGuid = GetCustomerId();

            return await _charactersRepository.GetCharByCharName(customerGuid, this.GetPrimaryKeyString());
        }

        public async Task<CustomCharacterDataRows> GetCustomData()
        {
            Guid customerGuid = GetCustomerId();

            CustomCharacterDataRows output = new CustomCharacterDataRows();

            output.Rows = await _charactersRepository.GetCustomCharacterData(customerGuid, this.GetPrimaryKeyString());

            return output;
        }

        public async Task UpdateCharacterPosition(string mapName)
        {
            //TODO: UpdateAllPlayerPositions probably merits a MapGrain
        }

        public async Task<SuccessAndErrorMessage> UpdateCharacterStats(UpdateCharacterStats stats)
        {
            SuccessAndErrorMessage successAndErrorMessage = new SuccessAndErrorMessage();
            successAndErrorMessage.Success = true;

            try
            {
                await _charactersRepository.UpdateCharacterStats(stats);
            }
            catch (Exception ex)
            {
                successAndErrorMessage.ErrorMessage = ex.Message;
                successAndErrorMessage.Success = false;
            }

            return successAndErrorMessage;
        }

        public async Task Logout()
        {
            Guid customerGuid = GetCustomerId();

            await _charactersRepository.PlayerLogout(customerGuid, this.GetPrimaryKeyString());
        }

        public async Task<SuccessAndErrorMessage> AddAbilityToCharacter(string abilityName, int abilityLevel, string charHasAbilitiesCustomJson)
        {
            Guid customerGuid = GetCustomerId();

            var output = new SuccessAndErrorMessage();
            await _charactersRepository.AddAbilityToCharacter(customerGuid, abilityName, characterName: this.GetPrimaryKeyString(), abilityLevel, charHasAbilitiesCustomJson);

            output.Success = true;
            output.ErrorMessage = "";

            return output;
        }

        public async Task<IEnumerable<GetCharacterAbilities>> GetCharacterAbilities()
        {
            Guid customerGuid = GetCustomerId();

            return await _charactersRepository.GetCharacterAbilities(customerGuid, characterName: this.GetPrimaryKeyString());
        }

        public async Task<IEnumerable<Abilities>> GetAbilities()
        {
            Guid customerGuid = GetCustomerId();

            return await _charactersRepository.GetAbilities(customerGuid);
        }

        public async Task<IEnumerable<GetAbilityBars>> GetAbilityBars()
        {
            Guid customerGuid = GetCustomerId();

            return await _charactersRepository.GetAbilityBars(customerGuid, this.GetPrimaryKeyString());
        }

        public async Task<IEnumerable<GetCharacterAbilities>> GetAbilityBarsAndAbilities()
        {
            Guid customerGuid = GetCustomerId();

            return await _charactersRepository.GetCharacterAbilities(customerGuid, this.GetPrimaryKeyString());
        }

        public async Task<SuccessAndErrorMessage> RemoveAbilityFromCharacter(string abilityName)
        {
            Guid customerGuid = GetCustomerId();

            var output = new SuccessAndErrorMessage();
            await _charactersRepository.RemoveAbilityFromCharacter(customerGuid, abilityName, this.GetPrimaryKeyString());

            output.Success = true;
            output.ErrorMessage = "";

            return output;
        }

        public async Task<SuccessAndErrorMessage> UpdateAbilityOnCharacter(string abilityName, int abilityLevel, string charHasAbilitiesCustomJson)
        {
            Guid customerGuid = GetCustomerId();
            
            var output = new SuccessAndErrorMessage();
            await _charactersRepository.UpdateAbilityOnCharacter(customerGuid, abilityName, this.GetPrimaryKeyString(), abilityLevel, charHasAbilitiesCustomJson);

            output.Success = true;
            output.ErrorMessage = "";

            return output;
        }
    }
}
