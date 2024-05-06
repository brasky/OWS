using Orleans;

namespace OWSData.Models.StoredProcs
{
    [GenerateSerializer]
    public class AddOrUpdateCustomCharacterData
    {
        [Id(0)] public string CharacterName { get; set; }
        [Id(1)] public string CustomFieldName { get; set; }
        [Id(2)] public string FieldValue { get; set; }
    }
}