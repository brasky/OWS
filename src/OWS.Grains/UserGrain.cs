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
    public class UserGrain : Grain, IUserGrain
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
            if (Guid.TryParse(RequestContext.Get("CustomerId") as string, out var customerGuid))
            {
                throw new ArgumentException("Invalid Customer ID");
            }

            var output = await _usersRepository.LoginAndCreateSession(customerGuid, email, password, false);

            if (!output.Authenticated || !output.UserSessionGuid.HasValue || output.UserSessionGuid == Guid.Empty)
            {
                output.ErrorMessage = "Username or Password is invalid!";
            }

            return output;
        }

        public async Task<GetUserSession> GetUserSessionAsync()
        {
            if (Guid.TryParse(RequestContext.Get("CustomerId") as string, out var customerGuid))
            {
                throw new ArgumentException("Invalid Customer ID");
            }

            var session = await _usersRepository.GetUserSession(customerGuid, this.GetPrimaryKey());
            return session;
        }

        public async Task<IEnumerable<GetAllCharacters>> GetAllCharacters()
        {
            if (Guid.TryParse(RequestContext.Get("CustomerId") as string, out var customerGuid))
            {
                throw new ArgumentException("Invalid Customer ID");
            }

            return await _usersRepository.GetAllCharacters(customerGuid, this.GetPrimaryKey());
        }

        public async Task<IEnumerable<GetPlayerGroupsCharacterIsIn>> GetPlayerGroupsCharacterIsIn(string characterName, int playerGroupTypeId)
        {
            if (Guid.TryParse(RequestContext.Get("CustomerId") as string, out var customerGuid))
            {
                throw new ArgumentException("Invalid Customer ID");
            }

            return await _usersRepository.GetPlayerGroupsCharacterIsIn(customerGuid, this.GetPrimaryKey(), characterName, playerGroupTypeId);
        }

        public async Task<SuccessAndErrorMessage> UserSessionSetSelectedCharacter(string selectedCharacterName)
        {
            if (Guid.TryParse(RequestContext.Get("CustomerId") as string, out var customerGuid))
            {
                throw new ArgumentException("Invalid Customer ID");
            }

            return await _usersRepository.UserSessionSetSelectedCharacter(customerGuid, this.GetPrimaryKey(), selectedCharacterName);
        }

        public async Task<GetUserSession> SetSelectedCharacterAndGetUserSession(string selectedCharacterName)
        {
            if (Guid.TryParse(RequestContext.Get("CustomerId") as string, out var customerGuid))
            {
                throw new ArgumentException("Invalid Customer ID");
            }

            var successOrError = await _usersRepository.UserSessionSetSelectedCharacter(customerGuid, this.GetPrimaryKey(), selectedCharacterName);

            var output = await _usersRepository.GetUserSession(customerGuid, this.GetPrimaryKey());

            return output;
        }

        public async Task<PlayerLoginAndCreateSession> RegisterUser(RegisterUserDTO registerUserDto)
        {
            if (Guid.TryParse(RequestContext.Get("CustomerId") as string, out var customerGuid))
            {
                throw new ArgumentException("Invalid Customer ID");
            }

            //Check for duplicate account before creating a new one:
            var foundUser = await _usersRepository.GetUserFromEmail(customerGuid, registerUserDto.Email);

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
            SuccessAndErrorMessage registerOutput = await _usersRepository.RegisterUser(customerGuid, registerUserDto.Email, registerUserDto.Password, registerUserDto.FirstName, registerUserDto.LastName);

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
            PlayerLoginAndCreateSession playerLoginAndCreateSession = await _usersRepository.LoginAndCreateSession(customerGuid, registerUserDto.Email, registerUserDto.Password);

            /*
            if (externalLoginProviderFactory != null)
            {
                //This method will do nothing if AutoRegister isn't set to true
                //await externalLoginProvider.RegisterAsync(Email, Password, Email);
            }
            */

            return playerLoginAndCreateSession;
        }

        public async Task Logout()
        {
            if (Guid.TryParse(RequestContext.Get("CustomerId") as string, out var customerGuid))
            {
                throw new ArgumentException("Invalid Customer ID");
            }

            await _usersRepository.Logout(customerGuid, this.GetPrimaryKey());
        }
    }
}
