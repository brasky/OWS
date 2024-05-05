using OWSData.Models.StoredProcs;

namespace OWS.Interfaces
{
    public interface ICharacterGrain : IGrainWithStringKey
    {
        Task<JoinMapByCharName> GetServerToConnectTo(Guid customerGuid, string zoneName, int playerGroupType);
    }
}
