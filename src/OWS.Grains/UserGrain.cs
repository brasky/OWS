using Microsoft.Extensions.Logging;
using OWSData.Models.StoredProcs;
using OWSData.Repositories.Interfaces;
using User.Interfaces;

namespace OWS.Grains
{
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

        public async Task<GetUserSession> GetUserSessionAsync(Guid customerGuid)
        {
            var session = await _usersRepository.GetUserSession(customerGuid, this.GetPrimaryKey());
            return session;
        }
    }
}
