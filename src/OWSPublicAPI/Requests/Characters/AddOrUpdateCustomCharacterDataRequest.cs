using OWSData.Models.StoredProcs;
using OWSData.Repositories.Interfaces;
using OWSShared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OWSCharacterPersistence.Requests.Characters
{
    public class AddOrUpdateCustomDataRequest
    {
        public AddOrUpdateCustomCharacterData addOrUpdateCustomCharacterData { get; set; }
    }
}
