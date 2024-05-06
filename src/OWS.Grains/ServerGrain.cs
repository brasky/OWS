using Microsoft.Extensions.Logging;
using OWS.Interfaces;
using OWSData.Repositories.Interfaces;
using OWSShared.Interfaces;
using OWSShared.Options;
using System.Text;

namespace OWS.Grains
{
    public sealed class ServerGrain : BaseGrain
    {
        private readonly ILogger<ServerGrain> _logger;
        private readonly IUsersRepository _usersRepository;
        private readonly IWritableOptions<OWSInstanceLauncherOptions> _owsInstanceLauncherOptions;
        private readonly IZoneServerProcessesRepository _zoneServerProcessesRepository;
        private readonly IOWSInstanceLauncherDataRepository _owsInstanceLauncherDataRepository;

        public ServerGrain(
            ILogger<ServerGrain> logger,
            IUsersRepository usersRepository,
            IWritableOptions<OWSInstanceLauncherOptions> owsInstanceLauncherOptions,
            IZoneServerProcessesRepository zoneServerProcessesRepository,
            IOWSInstanceLauncherDataRepository owsInstanceLauncherDataRepository)
        {
            _logger = logger;
            _usersRepository = usersRepository;
            _owsInstanceLauncherOptions = owsInstanceLauncherOptions;
            _zoneServerProcessesRepository = zoneServerProcessesRepository;
            _owsInstanceLauncherDataRepository = owsInstanceLauncherDataRepository;
        }
    }
}
