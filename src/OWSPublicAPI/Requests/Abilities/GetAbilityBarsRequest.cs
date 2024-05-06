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
    /// Get Ability Bars
    /// </summary>
    /// <remarks>
    /// Get a list of ability bars this character has.
    /// </remarks>
    public class GetAbilityBarsRequest
    {
        /// <summary>
        /// Character Name
        /// </summary>
        /// <remarks>
        /// This is the name of the character to get ability bars for.
        /// </remarks>
        public string CharacterName { get; set; }
    }
}
