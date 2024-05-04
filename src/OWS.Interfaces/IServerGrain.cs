using OWSData.Models.StoredProcs;

namespace User.Interfaces
{
    public interface IUserGrain : IGrainWithGuidKey
    {
        Task<GetUserSession> GetUserSessionAsync(Guid customerGuid);
    }
}
