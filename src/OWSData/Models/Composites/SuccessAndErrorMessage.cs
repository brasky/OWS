using Orleans;

namespace OWSData.Models.Composites
{
    [GenerateSerializer(GenerateFieldIds = GenerateFieldIds.PublicProperties)]
    public class SuccessAndErrorMessage
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
