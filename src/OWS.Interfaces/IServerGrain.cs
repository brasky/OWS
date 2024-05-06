using OWSData.Models.Composites;
using OWSData.Models.StoredProcs;
using OWSShared.DTOs;

namespace OWS.Interfaces
{
    public interface IUserGrain : IGrainWithGuidKey
    {
        Task<IEnumerable<GetAllCharacters>> GetAllCharacters();
        Task<IEnumerable<GetPlayerGroupsCharacterIsIn>> GetPlayerGroupsCharacterIsIn(string characterName, int playerGroupTypeId);
        Task<GetUserSession> GetUserSessionAsync();
        Task<PlayerLoginAndCreateSession> LoginAndCreateSession(string email, string password);
        Task Logout();
        Task<PlayerLoginAndCreateSession> RegisterUser(RegisterUserDTO registerUserDto);
        Task<GetUserSession> SetSelectedCharacterAndGetUserSession(string selectedCharacterName);
        Task<SuccessAndErrorMessage> UserSessionSetSelectedCharacter(string selectedCharacterName);
    }
}
