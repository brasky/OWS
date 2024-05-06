using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Orleans;
using OWS.Interfaces;
using OWSCharacterPersistence.Requests.Abilities;
using OWSData.Models.Composites;
using OWSData.Models.StoredProcs;
using OWSData.Models.Tables;
using OWSData.Repositories.Interfaces;
using OWSShared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWSCharacterPersistence.Controllers
{
    /// <summary>
    /// Ability related API calls.
    /// </summary>
    /// <remarks>
    /// Contains ability related API calls that are only accessible internally.
    [Route("api/[controller]")]
    [ApiController]
    public class AbilitiesController : Controller
    {
        private readonly IClusterClient _clusterClient;

        /// <summary>
        /// Constructor for Ability related API calls.
        /// </summary>
        /// <remarks>
        /// All dependencies are injected.
        /// </remarks>
        public AbilitiesController(IClusterClient clusterClient)
        {
            this._clusterClient = clusterClient;
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

        /*
        [HttpPost]
        [Route("GetAbilities")]
        [Produces(typeof(GetAbilities))]
        public async Task<IActionResult> GetAbilities([FromBody] GetAbilitiesRequest request)
        {
            request.SetData(_charactersRepository, _customerGuid);
            return await request.Handle();
        }
        */

        /// <summary>
        /// Add Ability To Character
        /// </summary>
        /// <remarks>
        /// Adds an Ability to a Character and also sets the initial values of Ability Level and the per instance Custom JSON
        /// </remarks>
        /// <param name="request">
        /// <b>AbilityName</b> - This is the name of the ability to add to the character.<br/>
        /// <b>AbilityLevel</b> - This is a number representing the Ability Level of the ability to add.  If you need more per instance customization, use the Custom JSON field.<br/>
        /// <b>CharacterName</b> - This is the name of the character to add the ability to.<br/>
        /// <b>CharHasAbilitiesCustomJSON</b> - This field is used to store Custom JSON for the specific instance of this Ability.  If you have a system where each ability on a character has some kind of custom variation, then this is where to store that variation data.  In a system where an ability operates the same on every player, this field would not be used.  Don't store Ability Level in this field, as there is already a field for that.  If you need to store Custom JSON for ALL instances of an ability, use the Custom JSON on the Ability itself.
        /// </param>
        [HttpPost]
        [Route("AddAbilityToCharacter")]
        [Produces(typeof(SuccessAndErrorMessage))]
        public async Task<SuccessAndErrorMessage> AddAbilityToCharacter([FromBody] AddAbilityToCharacterRequest request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.CharacterName);
            return await grain.AddAbilityToCharacter(request.AbilityName, request.AbilityLevel, request.CharHasAbilitiesCustomJSON);
        }

        /// <summary>
        /// Get Character Abilities
        /// </summary>
        /// <remarks>
        /// Gets a List of the Abilities on the Character specified with CharacterName
        /// </remarks>
        /// <param name="request">
        /// <b>CharacterName</b> - This is the name of the character to get abilities for.
        /// </param>
        [HttpPost]
        [Route("GetCharacterAbilities")]
        [Produces(typeof(IEnumerable<GetCharacterAbilities>))]
        public async Task<IActionResult> GetCharacterAbilities([FromBody] GetCharacterAbilitiesRequest request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.CharacterName);
            return new OkObjectResult(await grain.GetCharacterAbilities());
        }

        /// <summary>
        /// Get Abilities
        /// </summary>
        /// <remarks>
        /// Gets a List of all Abilities
        /// </remarks>
        [HttpGet]
        [Route("GetAbilities")]
        [Produces(typeof(IEnumerable<Abilities>))]
        public async Task<IEnumerable<Abilities>> GetAbilities()
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(Guid.NewGuid().ToString());
            return await grain.GetAbilities();
        }

        /// <summary>
        /// Get Ability Bars
        /// </summary>
        /// <remarks>
        /// Gets a List of Ability Bars for the Character specified with CharacterName
        /// </remarks>
        /// <param name="request">
        /// <b>CharacterName</b> - This is the name of the character to get ability bars for.
        /// </param>
        [HttpPost]
        [Route("GetAbilityBars")]
        [Produces(typeof(IEnumerable<GetAbilityBars>))]
        public async Task<IActionResult> GetAbilityBars([FromBody] GetAbilityBarsRequest request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.CharacterName);
            return new OkObjectResult(await grain.GetAbilityBars());
        }

        /// <summary>
        /// Get Ability Bars and Abilities
        /// </summary>
        /// <remarks>
        /// Gets a List of Ability Bars and the Abilities on those Bars for the Character specified with CharacterName
        /// </remarks>
        /// <param name="request">
        /// <b>CharacterName</b> - This is the name of the character to get abilities for.
        /// </param>
        [HttpPost]
        [Route("GetAbilityBarsAndAbilities")]
        [Produces(typeof(IEnumerable<GetAbilityBarsAndAbilities>))]
        public async Task<IActionResult> GetAbilityBarsAndAbilities([FromBody] GetCharacterAbilitiesRequest request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.CharacterName);
            return new OkObjectResult(await grain.GetAbilityBarsAndAbilities());
        }

        /// <summary>
        /// Remove Ability From Character
        /// </summary>
        /// <remarks>
        /// Removes an Ability from a Character
        /// </remarks>
        /// <param name="request">
        /// <b>AbilityName</b> - This is the name of the ability to add to the character.<br/>
        /// <b>CharacterName</b> - This is the name of the character to add the ability to.
        /// </param>
        [HttpPost]
        [Route("RemoveAbilityFromCharacter")]
        [Produces(typeof(SuccessAndErrorMessage))]
        public async Task<SuccessAndErrorMessage> RemoveAbilityFromCharacter([FromBody] RemoveAbilityFromCharacterRequest request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.CharacterName);
            return await grain.RemoveAbilityFromCharacter(request.AbilityName);
        }

        /// <summary>
        /// Update Ability on Character
        /// </summary>
        /// <remarks>
        /// Adds an Ability to a Character and also sets the initial values of Ability Level and the per instance Custom JSON
        /// </remarks>
        /// <param name="request">
        /// <b>AbilityName</b> - This is the name of the ability to update on the character.<br/>
        /// <b>AbilityLevel</b> - This is a number representing the Ability Level of the ability to add.  If you need more per instance customization, use the Custom JSON field.<br/>
        /// <b>CharacterName</b> - This is the name of the character to add the ability to.<br/>
        /// <b>CharHasAbilitiesCustomJSON</b> - This field is used to store Custom JSON for the specific instance of this Ability.  If you have a system where each ability on a character has some kind of custom variation, then this is where to store that variation data.  In a system where an ability operates the same on every player, this field would not be used.  Don't store Ability Level in this field, as there is already a field for that.  If you need to store Custom JSON for ALL instances of an ability, use the Custom JSON on the Ability itself.
        /// </param>
        [HttpPost]
        [Route("UpdateAbilityOnCharacter")]
        [Produces(typeof(SuccessAndErrorMessage))]
        public async Task<SuccessAndErrorMessage> UpdateAbilityOnCharacter([FromBody] UpdateAbilityOnCharacterRequest request)
        {
            var grain = _clusterClient.GetGrain<ICharacterGrain>(request.CharacterName);
            return await grain.UpdateAbilityOnCharacter(request.AbilityName, request.AbilityLevel, request.CharHasAbilitiesCustomJSON);
        }
    }
}
