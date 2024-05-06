using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OWS.Interfaces;
using OWSData.Models.Composites;
using OWSData.Models.StoredProcs;
using OWSData.Repositories.Interfaces;
using OWSShared.Messages;
using OWSShared.Options;
using OWSShared.RequestPayloads;
using RabbitMQ.Client;

namespace OWS.Grains
{
    public sealed class InstanceGrain : BaseGrain, IInstanceGrain
    {
        private readonly ILogger<ServerGrain> _logger;
        private readonly IInstanceManagementRepository _instanceManagementRepository;
        private readonly ICharactersRepository _charactersRepository;
        private readonly IOptions<RabbitMQOptions> _rabbitMQOptions;

        public InstanceGrain(
            ILogger<ServerGrain> logger,
            IInstanceManagementRepository instanceManagementRepository,
            ICharactersRepository charactersRepository,
            IOptions<RabbitMQOptions> rabbitMQOptions)
        {
            _logger = logger;
            _instanceManagementRepository = instanceManagementRepository;
            _charactersRepository = charactersRepository;
            _rabbitMQOptions = rabbitMQOptions;
        }

        public async Task<SuccessAndErrorMessage> SetZoneInstanceStatusRequest(int zoneInstanceId, int instanceStatus)
        {
            await _instanceManagementRepository.SetZoneInstanceStatus(GetCustomerId(), zoneInstanceId, instanceStatus);

            var output = new SuccessAndErrorMessage()
            {
                Success = true,
                ErrorMessage = ""
            };

            return output;
        }

        public Task<SuccessAndErrorMessage> ShutDownServerInstance(int worldServerId, int zoneInstanceId)
        {
            //Set the ZoneInstance status to shutting down


            //Send the servershutdown message to RabbitMQ
            ConnectionFactory factory = new()
            {
                HostName = _rabbitMQOptions.Value.RabbitMQHostName,
                Port = _rabbitMQOptions.Value.RabbitMQPort,
                UserName = _rabbitMQOptions.Value.RabbitMQUserName,
                Password = _rabbitMQOptions.Value.RabbitMQPassword
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "ows.servershutdown",
                        type: "direct",
                        durable: false,
                        autoDelete: false);

                    MQShutDownServerMessage serverSpinUpMessage = new()
                    {
                        CustomerGUID = GetCustomerId(),
                        ZoneInstanceID = zoneInstanceId
                    };

                    var body = serverSpinUpMessage.Serialize();

                    channel.BasicPublish(exchange: "ows.servershutdown",
                                         routingKey: string.Format("ows.servershutdown.{0}", worldServerId),
                                         basicProperties: null,
                                         body: body);
                }
            }

            var output = new SuccessAndErrorMessage()
            {
                Success = true,
                ErrorMessage = ""
            };

            return Task.FromResult(output);
        }

        public async Task<SuccessAndErrorMessage> RegisterLauncher(RegisterInstanceLauncherRequestPayload Request)
        {
            return await _instanceManagementRepository.RegisterLauncher(GetCustomerId(), Request.launcherGUID, Request.ServerIP, Request.MaxNumberOfInstances, Request.InternalServerIP, Request.StartingInstancePort);
        }

        public Task<SuccessAndErrorMessage> SpinUpServerInstance(int worldServerId, int zoneInstanceId, string zoneName, int port)
        {
            ConnectionFactory factory = new()
            {
                HostName = _rabbitMQOptions.Value.RabbitMQHostName,
                Port = _rabbitMQOptions.Value.RabbitMQPort,
                UserName = _rabbitMQOptions.Value.RabbitMQUserName,
                Password = _rabbitMQOptions.Value.RabbitMQPassword
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "ows.serverspinup",
                        type: "direct",
                        durable: false,
                        autoDelete: false);

                    MQSpinUpServerMessage serverSpinUpMessage = new()
                    {
                        CustomerGUID = GetCustomerId(),
                        WorldServerID = worldServerId,
                        ZoneInstanceID = zoneInstanceId,
                        MapName = zoneName,
                        Port = port
                    };

                    var body = serverSpinUpMessage.Serialize();

                    channel.BasicPublish(exchange: "ows.serverspinup",
                                         routingKey: String.Format("ows.serverspinup.{0}", worldServerId),
                                         basicProperties: null,
                                         body: body);
                }
            }

            var Output = new SuccessAndErrorMessage()
            {
                Success = true,
                ErrorMessage = ""
            };

            return Task.FromResult(Output);
        }

        public async Task<int> StartInstanceLauncher(string launcherId)
        {
            return await _instanceManagementRepository.StartWorldServer(GetCustomerId(), launcherId);
        }

        public async Task<SuccessAndErrorMessage> ShutDownInstanceLauncher(int worldServerId)
        {
            return await _instanceManagementRepository.ShutDownWorldServer(GetCustomerId(), worldServerId);
        }

        public async Task<GetServerInstanceFromPort> GetZoneInstance(int zoneInstanceId)
        {
            return await _instanceManagementRepository.GetZoneInstance(GetCustomerId(), zoneInstanceId);
        }

        public async Task<GetServerInstanceFromPort> GetServerInstanceFromPort(string ipAddress, int port)
        {
            return await _instanceManagementRepository.GetServerInstanceFromPort(GetCustomerId(), ipAddress, port);
        }

        public async Task<IEnumerable<GetZoneInstancesForWorldServer>> GetZoneInstancesForWorldServer(int worldServerId)
        {
            return await _instanceManagementRepository.GetZoneInstancesForWorldServer(GetCustomerId(), worldServerId);
        }

        public async Task<SuccessAndErrorMessage> UpdateNumberOfPlayers(int zoneInstanceId, int numPlayers)
        {
            return await _instanceManagementRepository.UpdateNumberOfPlayers(GetCustomerId(), zoneInstanceId, numPlayers);
        }

        public async Task<IEnumerable<GetZoneInstancesForZone>> GetZoneInstancesForWorldServer(string zoneName)
        {
            return await _instanceManagementRepository.GetZoneInstancesOfZone(GetCustomerId(), zoneName);
        }

        public async Task<GetCurrentWorldTime> GetCurrentWorldTime()
        {
            return await _instanceManagementRepository.GetCurrentWorldTime(GetCustomerId());
        }

        public async Task<SuccessAndErrorMessage> AddOrUpdateZone(AddOrUpdateZoneRequestPayload request)
        {
            return await _instanceManagementRepository.AddZone(
                GetCustomerId(),
                request.MapName,
                request.ZoneName,
                request.WorldCompContainsFilter,
                request.WorldCompListFilter,
                request.SoftPlayerCap,
                request.HardPlayerCap,
                request.MapMode);
        }
    }
}
