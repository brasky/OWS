using Orleans;
using OWSData.Models.StoredProcs;
using System.Collections.Generic;

namespace OWSData.Models.Composites
{
    [GenerateSerializer]
    public class CharacterAndCustomData
    {
        [Id(0)] public GetCharByCharName CharacterData { get; set; }
        [Id(1)] public List<CustomCharacterDataDTO> CustomCharacterDataRows { get; set; }
}
}
