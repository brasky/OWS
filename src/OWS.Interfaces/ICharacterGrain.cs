using OWSData.Models.Composites;
using OWSData.Models.StoredProcs;

namespace OWS.Interfaces
{
    public interface ICharacterGrain : IGrainWithStringKey
    {
        Task<CharacterAndCustomData> PublicGetByNameRequest(Guid userSessionId);
        Task<GetCharByCharName> GetByName();
        Task<JoinMapByCharName> GetServerToConnectTo(string zoneName, int playerGroupType);
        Task AddOrUpdateCustomData(AddOrUpdateCustomCharacterData request);
        Task<CustomCharacterDataRows> GetCustomData();
        Task<SuccessAndErrorMessage> UpdateCharacterStats(UpdateCharacterStats stats);
        Task Logout();
        Task<CreateCharacter> Create(Guid userSessionId, string className);
    }
}
