using OWSData.Models.Composites;
using OWSData.Models.StoredProcs;
using OWSData.Models.Tables;

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
        Task<SuccessAndErrorMessage> CreateUsingDefaultCharacterValues(Guid userSessionId, string defaultSetName);
        Task<SuccessAndErrorMessage> AddAbilityToCharacter(string abilityName, int abilityLevel, string charHasAbilitiesCustomJson);
        Task<IEnumerable<GetCharacterAbilities>> GetCharacterAbilities();
        Task<IEnumerable<Abilities>> GetAbilities();
        Task<IEnumerable<GetAbilityBars>> GetAbilityBars();
        Task<IEnumerable<GetCharacterAbilities>> GetAbilityBarsAndAbilities();
        Task<SuccessAndErrorMessage> RemoveAbilityFromCharacter(string abilityName);
        Task<SuccessAndErrorMessage> UpdateAbilityOnCharacter(string abilityName, int abilityLevel, string charHasAbilitiesCustomJson);
    }
}
