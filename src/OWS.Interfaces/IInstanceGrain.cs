using OWSData.Models.Composites;
using OWSData.Models.StoredProcs;
using OWSShared.RequestPayloads;

namespace OWS.Interfaces
{
    public interface IInstanceGrain : IGrainWithGuidKey
    {
        Task<SuccessAndErrorMessage> AddOrUpdateZone(AddOrUpdateZoneRequestPayload request);
        Task<GetCurrentWorldTime> GetCurrentWorldTime();
        Task<GetServerInstanceFromPort> GetServerInstanceFromPort(string ipAddress, int port);
        Task<GetServerInstanceFromPort> GetZoneInstance(int zoneInstanceId);
        Task<IEnumerable<GetZoneInstancesForWorldServer>> GetZoneInstancesForWorldServer(int worldServerId);
        Task<IEnumerable<GetZoneInstancesForZone>> GetZoneInstancesForWorldServer(string zoneName);
        Task<SuccessAndErrorMessage> RegisterLauncher(RegisterInstanceLauncherRequestPayload Request);
        Task<SuccessAndErrorMessage> SetZoneInstanceStatusRequest(int zoneInstanceId, int instanceStatus);
        Task<SuccessAndErrorMessage> ShutDownInstanceLauncher(int worldServerId);
        Task<SuccessAndErrorMessage> ShutDownServerInstance(int worldServerId, int zoneInstanceId);
        Task<SuccessAndErrorMessage> SpinUpServerInstance(int worldServerId, int zoneInstanceId, string zoneName, int port);
        Task<int> StartInstanceLauncher(string launcherId);
        Task<SuccessAndErrorMessage> UpdateNumberOfPlayers(int zoneInstanceId, int numPlayers);
    }
}
