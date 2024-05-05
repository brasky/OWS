using Microsoft.Extensions.Logging;
using OWSData.Models.StoredProcs;
using OWSData.Repositories.Interfaces;
using OWS.Interfaces;
using Orleans.Runtime;

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

        public async Task<GetUserSession> GetUserSessionAsync()
        {
            if (Guid.TryParse(RequestContext.Get("CustomerId") as string, out var customerGuid))
            {
                throw new ArgumentException("Invalid Customer ID");
            }

            var session = await _usersRepository.GetUserSession(customerGuid, this.GetPrimaryKey());
            return session;
        }
    }
}
