using OWSData.Models.StoredProcs;

namespace User.Interfaces
{
    public interface ICharacterGrain : IGrainWithStringKey
    {
        Task<JoinMapByCharName> GetServerToConnectTo(Guid customerGuid, string zoneName, int playerGroupType);
    }
}
