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

namespace OWS.Grains
{
    public class CharacterGrain : Grain, ICharacterGrain
    {
        private readonly ILogger<CharacterGrain> _logger;
        private readonly IUsersRepository _usersRepository;
        private readonly ICharactersRepository _charactersRepository;
        private readonly IOptions<RabbitMQOptions> _rabbitMQOptions;

        public CharacterGrain(

            ILogger<CharacterGrain> logger,
            IUsersRepository usersRepository,
            ICharactersRepository charactersRepository,
            IOptions<RabbitMQOptions> rabbitMQOptions)
        {
            _logger = logger;
            _usersRepository = usersRepository;
            _charactersRepository = charactersRepository;
            _rabbitMQOptions = rabbitMQOptions;
        }

        public async Task<JoinMapByCharName> GetServerToConnectTo(Guid customerGuid, string zoneName, int playerGroupType)
        {
            var Output = new JoinMapByCharName();

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

                //var serverGrain = GrainFactory.GetGrain<IServerGrain>(joinMapByCharacterName.WorldServerID);
                //await serverGrain.RequestServerSpinUpAsync(
                //    customerGuid: customerGuid,
                //    zoneInstanceId: joinMapByCharacterName.MapInstanceID,
                //    zoneName: joinMapByCharacterName.MapNameToStart,
                //    port: joinMapByCharacterName.Port);
                //bool requestSuccess = await RequestServerSpinUp(joinMapByCharacterName.WorldServerID, joinMapByCharacterName.MapInstanceID, joinMapByCharacterName.MapNameToStart, joinMapByCharacterName.Port);\


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
                //Wait OWSGeneralConfig.SecondsToWaitBeforeFirstPollForSpinUp seconds before the first CheckMapInstanceStatus to give it time to spin up

                //Thread.Sleep(owsGeneralConfig.Value.SecondsToWaitBeforeFirstPollForSpinUp);

                //readyForPlayersToConnect = await WaitForServerReadyToConnect(CustomerGUID, joinMapByCharacterName.MapInstanceID);
            }
            //We found a zone server we can connect to, but it is still spinning up.  Wait until it is ready to connect to (up to OWSGeneralConfig.SecondsToWaitForServerSpinUp seconds).
            else if (joinMapByCharacterName.MapInstanceID > 0 && joinMapByCharacterName.MapInstanceStatus == 1)
            {
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
    }
}
