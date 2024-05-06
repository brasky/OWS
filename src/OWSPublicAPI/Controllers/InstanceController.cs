using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using OWSData.Models.StoredProcs;
using OWSShared.Interfaces;
using OWSInstanceManagement.Requests.Instance;
using OWSData.Models.Composites;
using OWSData.Repositories.Interfaces;
using OWSShared.Options;
using Serilog;
using Orleans;
using OWS.Interfaces;

namespace OWSInstanceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstanceController : Controller
    {
        private readonly IClusterClient _clusterClient;

        public InstanceController(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
        }

        [HttpPost]
        [Route("SetZoneInstanceStatus")]
        [Produces(typeof(SuccessAndErrorMessage))]
        /*[SwaggerOperation("ByName")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]*/
        public async Task<IActionResult> SetZoneInstanceStatusRequest([FromBody] SetZoneInstanceStatusRequest request)
        {
            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return new OkObjectResult(await grain.SetZoneInstanceStatusRequest(request.request.ZoneInstanceID, request.request.InstanceStatus));
        }

        [HttpPost]
        [Route("ShutDownServerInstance")]
        [Produces(typeof(SuccessAndErrorMessage))]
        /*[SwaggerOperation("ByName")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]*/
        public async Task<IActionResult> ShutDownServerInstance([FromBody] ShutDownServerInstanceRequest request)
        {
            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return new OkObjectResult(await grain.ShutDownServerInstance(request.WorldServerID, request.ZoneInstanceID));
        }

        [HttpPost]
        [Route("RegisterLauncher")]
        [Produces(typeof(SuccessAndErrorMessage))]
        /*[SwaggerOperation("ByName")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]*/
        public async Task<SuccessAndErrorMessage> RegisterLauncher([FromBody] RegisterInstanceLauncherRequest request)
        {
            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return await grain.RegisterLauncher(request.Request);
        }

        [HttpPost]
        [Route("SpinUpServerInstance")]
        [Produces(typeof(SuccessAndErrorMessage))]
        /*[SwaggerOperation("ByName")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]*/
        public async Task<IActionResult> SpinUpServerInstance([FromBody] SpinUpServerInstanceRequest request)
        {
            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return new OkObjectResult(await grain.SpinUpServerInstance(request.WorldServerID, request.ZoneInstanceID, request.ZoneName, request.Port));
        }

        [HttpGet]
        [Route("StartInstanceLauncher")]
        [Produces(typeof(int))]
        /*[SwaggerOperation("ByName")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]*/
        public async Task<IActionResult> StartInstanceLauncher()
        {
            StartInstanceLauncherRequest request = new StartInstanceLauncherRequest();

            var launcherGuid = Request.Headers["X-LauncherGUID"].FirstOrDefault();
            if (string.IsNullOrEmpty(launcherGuid))
            {
                Log.Error("Http Header X-LauncherGUID is empty!");
            }

            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return new OkObjectResult(await grain.StartInstanceLauncher(launcherGuid));
        }

        [HttpPost]
        [Route("ShutDownInstanceLauncher")]
        [Produces(typeof(SuccessAndErrorMessage))]
        /*[SwaggerOperation("ByName")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]*/
        public async Task<IActionResult> ShutDownInstanceLauncher([FromBody] ShutDownInstanceLauncherRequest request)
        {
            var launcherGuid = Request.Headers["X-LauncherGUID"].FirstOrDefault();
            if (string.IsNullOrEmpty(launcherGuid))
            {
                Log.Error("Http Header X-LauncherGUID is empty!");
            }

            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return new OkObjectResult(await grain.ShutDownInstanceLauncher(request.Request.WorldServerID));
        }

        /// <summary>
        /// GetZoneInstance
        /// </summary>
        /// <remarks>
        /// Get information on the server instance that matches the ZoneInstanceId in the POST data
        /// </remarks>
        [HttpPost]
        [Route("GetZoneInstance")]
        [Produces(typeof(GetServerInstanceFromPort))]
        public async Task<GetServerInstanceFromPort> GetZoneInstance([FromBody] int ZoneInstanceId)
        {
            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return await grain.GetZoneInstance(ZoneInstanceId);
        }

        [HttpPost]
        [Route("GetServerInstanceFromPort")]
        [Produces(typeof(GetServerInstanceFromPort))]
        /*[SwaggerOperation("ByName")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]*/
        public async Task<GetServerInstanceFromPort> GetServerInstanceFromPort([FromBody] GetServerInstanceFromPortRequest request)
        {
            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return await grain.GetServerInstanceFromPort(Request.HttpContext.Connection.RemoteIpAddress.ToString(), request.Port);
        }

        [HttpPost]
        [Route("GetZoneInstancesForWorldServer")]
        [Produces(typeof(IEnumerable<GetZoneInstancesForWorldServer>))]
        /*[SwaggerOperation("ByName")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]*/
        public async Task<IActionResult> GetZoneInstancesForWorldServer([FromBody] GetZoneInstancesForWorldServerRequest request)
        {
            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return new OkObjectResult(await grain.GetZoneInstancesForWorldServer(request.Request.WorldServerID));
        }

        [HttpPost]
        [Route("UpdateNumberOfPlayers")]
        [Produces(typeof(SuccessAndErrorMessage))]
        /*[SwaggerOperation("ByName")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]*/
        public async Task<IActionResult> UpdateNumberOfPlayers([FromBody] UpdateNumberOfPlayersRequest request)
        {
            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return new OkObjectResult(await grain.UpdateNumberOfPlayers(request.ZoneInstanceId, request.NumberOfConnectedPlayers));
        }

        [HttpPost]
        [Route("GetZoneInstancesForZone")]
        [Produces(typeof(IEnumerable<GetZoneInstancesForZone>))]
        public async Task<IActionResult> GetZoneInstancesForZone([FromBody] GetZoneInstancesForZoneRequest request)
        {
            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return new OkObjectResult(await grain.GetZoneInstancesForWorldServer(request.Request.ZoneName));
        }

        [HttpPost]
        [Route("GetCurrentWorldTime")]
        [Produces(typeof(GetCurrentWorldTime))]
        public async Task<IActionResult> GetCurrentWorldTime([FromBody] GetCurrentWorldTimeRequest request)
        {
            var grain = _clusterClient.GetGrain<IInstanceGrain>(Guid.NewGuid());
            return new OkObjectResult(await grain.GetCurrentWorldTime());
        }
    }
}