using Orleans;

namespace OWSData.Models.StoredProcs
{
    [GenerateSerializer(GenerateFieldIds = GenerateFieldIds.PublicProperties)]
    public class GetCurrentWorldTime
    {
        public long CurrentWorldTime { get; set; }

    }
}
