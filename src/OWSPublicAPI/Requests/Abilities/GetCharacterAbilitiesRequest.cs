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
    /// Get Character Abilities
    /// </summary>
    /// <remarks>
    /// Get a list of the abilities this character has.
    /// </remarks>
    public class GetCharacterAbilitiesRequest
    {
        /// <summary>
        /// Character Name
        /// </summary>
        /// <remarks>
        /// This is the name of the character to get abilities for.
        /// </remarks>
        public string CharacterName { get; set; }
    }
}
