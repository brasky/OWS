namespace User.Interfaces
{
    public interface IServerGrain : IGrainWithIntegerKey
    {
        Task RequestServerSpinUpAsync(Guid customerGuid, int zoneInstanceId, string zoneName, int port);
    }
}
