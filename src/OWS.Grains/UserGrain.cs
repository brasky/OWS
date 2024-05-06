using Microsoft.Extensions.Logging;
using OWSData.Models.StoredProcs;
using OWSData.Repositories.Interfaces;
using OWS.Interfaces;
using Orleans.Runtime;
using Microsoft.AspNetCore.Mvc;
using OWSData.Models.Tables;
using OWSData.Models.Composites;
using OWSShared.DTOs;

namespace OWS.Grains
{
    //Primary key is UserSessionId
    public sealed class UserGrain : BaseGrain, IUserGrain
    {
        private readonly ILogger<UserGrain> _logger;
        private readonly IUsersRepository _usersRepository;

        public UserGrain(
            ILogger<UserGrain> logger,
            IUsersRepository usersRepository)
        {
            _logger = logger;
            _usersRepository = usersRepository;
        }

        public async Task<PlayerLoginAndCreateSession> LoginAndCreateSession(string email, string password)
        {
            var output = await _usersRepository.LoginAndCreateSession(GetCustomerId(), email, password, false);

            if (!output.Authenticated || !output.UserSessionGuid.HasValue || output.UserSessionGuid == Guid.Empty)
            {
                output.ErrorMessage = "Username or Password is invalid!";
            }

            return output;
        }

        public async Task<GetUserSession> GetUserSessionAsync()
        {
            var session = await _usersRepository.GetUserSession(GetCustomerId(), this.GetPrimaryKey());
            return session;
        }

        public async Task<IEnumerable<GetAllCharacters>> GetAllCharacters()
        {
            return await _usersRepository.GetAllCharacters(GetCustomerId(), this.GetPrimaryKey());
        }

        public async Task<IEnumerable<GetPlayerGroupsCharacterIsIn>> GetPlayerGroupsCharacterIsIn(string characterName, int playerGroupTypeId)
        {
            return await _usersRepository.GetPlayerGroupsCharacterIsIn(GetCustomerId(), this.GetPrimaryKey(), characterName, playerGroupTypeId);
        }

        public async Task<SuccessAndErrorMessage> UserSessionSetSelectedCharacter(string selectedCharacterName)
        {
            return await _usersRepository.UserSessionSetSelectedCharacter(GetCustomerId(), this.GetPrimaryKey(), selectedCharacterName);
        }

        public async Task<GetUserSession> SetSelectedCharacterAndGetUserSession(string selectedCharacterName)
        {
            var successOrError = await _usersRepository.UserSessionSetSelectedCharacter(GetCustomerId(), this.GetPrimaryKey(), selectedCharacterName);

            var output = await _usersRepository.GetUserSession(GetCustomerId(), this.GetPrimaryKey());

            return output;
        }

        public async Task<PlayerLoginAndCreateSession> RegisterUser(RegisterUserDTO registerUserDto)
        {
            var customerId = GetCustomerId();
            //Check for duplicate account before creating a new one:
            var foundUser = await _usersRepository.GetUserFromEmail(customerId, registerUserDto.Email);

            //This user already exists
            if (foundUser != null)
            {
                PlayerLoginAndCreateSession errorOutput = new PlayerLoginAndCreateSession()
                {
                    ErrorMessage = "Duplicate Account!"
                };

                return errorOutput;
            }

            //Register the new account
            SuccessAndErrorMessage registerOutput = await _usersRepository.RegisterUser(customerId, registerUserDto.Email, registerUserDto.Password, registerUserDto.FirstName, registerUserDto.LastName);

            //There was an error registering the new account
            if (!registerOutput.Success)
            {
                PlayerLoginAndCreateSession errorOutput = new PlayerLoginAndCreateSession()
                {
                    ErrorMessage = registerOutput.ErrorMessage
                };

                return errorOutput;
            }

            //Login to the new account to get a UserSession
            PlayerLoginAndCreateSession playerLoginAndCreateSession = await _usersRepository.LoginAndCreateSession(customerId, registerUserDto.Email, registerUserDto.Password);

            /*
            if (externalLoginProviderFactory != null)
            {
                //This method will do nothing if AutoRegister isn't set to true
                //await externalLoginProvider.RegisterAsync(Email, Password, Email);
            }
            */

            return playerLoginAndCreateSession;
        }

        public async Task<SuccessAndErrorMessage> RemoveCharacter(string characterName)
        {
            return await _usersRepository.RemoveCharacter(GetCustomerId(), this.GetPrimaryKey(), characterName);
        }

        public async Task Logout()
        {
            await _usersRepository.Logout(GetCustomerId(), this.GetPrimaryKey());
        }
    }
}
