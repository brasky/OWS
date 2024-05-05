using OWSData.Models.StoredProcs;

namespace OWS.Interfaces
{
    public interface IUserGrain : IGrainWithGuidKey
    {
        Task<GetUserSession> GetUserSessionAsync();
    }
}
