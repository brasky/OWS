using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Orleans;
using OWS.Interfaces;
using OWSCharacterPersistence.Requests.Characters;
using OWSData.Models.Composites;
using OWSData.Models.StoredProcs;
using OWSPublicAPI.Requests.Characters;
using OWSShared.DTOs;
using System;
using System.Threading.Tasks;

namespace OWSPublicAPI.Controllers
{
    /// <summary>
    /// Public Character related API calls.
    /// </summary>
    /// <remarks>
    /// Contains character related API calls that are all publicly accessible.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class CharactersController : Controller
    {
        private readonly IClusterClient _clusterClient;

        /// <summary>
        /// Constructor for Public Character related API calls.
        /// </summary>
        /// <remarks>
        /// All dependencies are injected.
        /// </remarks>
        public CharactersController(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        /// <summary>
        /// OnActionExecuting override
        /// </summary>
        /// <remarks>
        /// Checks for an empty IHeaderCustomerGUID.
        /// </remarks>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
        }

        /// <summary>
        /// Get Characters Data by Character Name.
        /// </summary>
        /// <remarks>
        /// Gets a Characters by Name.
        /// </remarks>
        [HttpPost]
        [Route("ByName")]
        [Produces(typeof(GetCharByCharName))]
        /*[SwaggerOperation("ByName")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]*/
        public async Task<IActionResult> PublicGetByNameRequest([FromBody] GetByNameDTO request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.CharacterName);
            if (!Guid.TryParse(request.UserSessionGUID, out var userSessionId))
            {
                return BadRequest("Invalid User Session Id");
            }
            return new OkObjectResult(await grain.PublicGetByNameRequest(userSessionId));
        }

        [HttpPost]
        [Route("AddOrUpdateCustomData")]
        public async Task AddOrUpdateCustomData([FromBody] AddOrUpdateCustomDataRequest request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.addOrUpdateCustomCharacterData.CharacterName);
            await grain.AddOrUpdateCustomData(request.addOrUpdateCustomCharacterData);
        }

        [HttpPost]
        [Route("GetByName")]
        [Produces(typeof(GetCharByCharName))]
        public async Task<IActionResult> GetByName([FromBody] GetByNameRequest request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.CharacterName);
            return new OkObjectResult(await grain.GetByName());
        }

        [HttpPost]
        [Route("GetCustomData")]
        [Produces(typeof(CustomCharacterDataRows))]
        public async Task<CustomCharacterDataRows> GetCustomData([FromBody] GetCustomDataRequest request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.CharacterName);
            return await grain.GetCustomData();
        }

        [HttpPost]
        [Route("UpdateAllPlayerPositions")]
        [Produces(typeof(SuccessAndErrorMessage))]
        public async Task<SuccessAndErrorMessage> UpdateAllPlayerPositions([FromBody] UpdateAllPlayerPositionsRequest request)
        {
            //TODO: merits a zonegrain
            //request.SetData(_charactersRepository, _customerGuid);
            return await request.Handle();
        }

        [HttpPost]
        [Route("UpdateCharacterStats")]
        [Produces(typeof(SuccessAndErrorMessage))]
        public async Task<SuccessAndErrorMessage> UpdateCharacterStats([FromBody] UpdateCharacterStatsRequest request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.updateCharacterStats.CharName);
            return await grain.UpdateCharacterStats(request.updateCharacterStats);
        }

        [HttpPost]
        [Route("PlayerLogout")]
        [Produces(typeof(SuccessAndErrorMessage))]
        public async Task<SuccessAndErrorMessage> PlayerLogout([FromBody] PlayerLogoutRequest request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.CharacterName);
            await grain.Logout();

            //Just doing this to keep the client API the same
            //why would we return an error logging out?
            SuccessAndErrorMessage output = new SuccessAndErrorMessage();
            output.Success = true;
            output.ErrorMessage = "";
            return output;
        }
    }
}
