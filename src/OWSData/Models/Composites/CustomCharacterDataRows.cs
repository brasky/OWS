using Orleans;
using OWSData.Models.Tables;
using System.Collections.Generic;

namespace OWSData.Models.Composites
{
    [GenerateSerializer]
    public class CustomCharacterDataRows
    {
        [Id(0)] public IEnumerable<CustomCharacterData> Rows { get; set; }
    }
}
