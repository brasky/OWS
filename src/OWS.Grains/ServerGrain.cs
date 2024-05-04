using Microsoft.Extensions.Logging;
using OWSData.Repositories.Interfaces;
using OWSShared.Interfaces;
using OWSShared.Options;
using Serilog;
using System.Text;
using User.Interfaces;

namespace OWS.Grains
{
    public class ServerGrain : Grain, IServerGrain
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
            //_userSession = userSession;
            _logger = logger;
            _usersRepository = usersRepository;
            _owsInstanceLauncherOptions = owsInstanceLauncherOptions;
            _zoneServerProcessesRepository = zoneServerProcessesRepository;
            _owsInstanceLauncherDataRepository = owsInstanceLauncherDataRepository;
        }

        public async Task RequestServerSpinUpAsync(Guid customerGuid, int zoneInstanceId, string zoneName, int port)
        {
            var worldServerId = this.GetPrimaryKeyLong();
            _logger.LogInformation($"Starting up {customerGuid} : {worldServerId} : {zoneName} : {port}");

            //string PathToDedicatedServer = "E:\\Program Files\\Epic Games\\UE_4.25\\Engine\\Binaries\\Win64\\UE4Editor.exe";
            //string ServerArguments = "\"C:\\OWS\\OpenWorldStarterPlugin\\OpenWorldStarter.uproject\" {0}?listen -server -log -nosteam -messaging -port={1}";

            string serverArguments = (_owsInstanceLauncherOptions.Value.IsServerEditor ? "\"" + _owsInstanceLauncherOptions.Value.PathToUProject + "\" " : "")
            + "{0}?listen -server "
                + (_owsInstanceLauncherOptions.Value.UseServerLog ? "-log " : "")
                + (_owsInstanceLauncherOptions.Value.UseNoSteam ? "-nosteam " : "")
                + "-port={1} "
                + "-zoneinstanceid={2}";

            System.Diagnostics.Process proc = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _owsInstanceLauncherOptions.Value.PathToDedicatedServer,
                    Arguments = Encoding.Default.GetString(Encoding.UTF8.GetBytes(String.Format(serverArguments, zoneName, port, zoneInstanceId))),
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                }
            };

            proc.Start();
            //proc.WaitForInputIdle();

            _zoneServerProcessesRepository.AddZoneServerProcess(new ZoneServerProcess
            {
                ZoneInstanceId = zoneInstanceId,
                MapName = zoneName,
                Port = port,
                ProcessId = proc.Id,
                ProcessName = proc.ProcessName
            });

            _logger.LogInformation($"{customerGuid} : {worldServerId} : {zoneName} : {port} has started.");

        }
    }
}
