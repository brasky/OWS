using Microsoft.AspNetCore.Mvc;
using OWSData.Repositories.Interfaces;
using OWSShared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWSCharacterPersistence.Requests.Abilities
{
    /// <summary>
    /// Get Ability Bars And Abilities
    /// </summary>
    /// <remarks>
    /// Get a flattened list of ability bars and the abilities on those bars for this character.
    /// </remarks>
    public class GetAbilityBarsAndAbilitiesRequest
    {
        /// <summary>
        /// Character Name
        /// </summary>
        /// <remarks>
        /// This is the name of the character to get ability bars and abilities for.
        /// </remarks>
        public string CharacterName { get; set; }
    }
}
