using Orleans;

namespace OWSData.Models.Composites
{
    [GenerateSerializer(GenerateFieldIds = GenerateFieldIds.PublicProperties)]

    public class GetWorldServerID
    {
        public int WorldServerID { get; set; }
    }
}
