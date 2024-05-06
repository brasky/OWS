using Orleans;

namespace OWSData.Models.Composites
{
    [GenerateSerializer]
    public class CreateCharacter
    {
        [Id(0)] public bool Success { get; set; }
        [Id(1)] public string ErrorMessage { get; set; }

        [Id(2)] public string CharacterName { get; set; }
        [Id(3)] public string ClassName { get; set; }
        [Id(4)] public int CharacterLevel { get; set; }
        [Id(5)] public string StartingMapName { get; set; }
        [Id(6)] public float X { get; set; }
        [Id(7)] public float Y { get; set; }
        [Id(8)] public float Z { get; set; }
        [Id(9)] public float RX { get; set; }
        [Id(10)] public float RY { get; set; }
        [Id(11)] public float RZ { get; set; }
        [Id(12)] public int TeamNumber { get; set; }
        [Id(13)] public int Gold { get; set; }
        [Id(14)] public int Silver { get; set; }
        [Id(15)] public int Copper { get; set; }
        [Id(16)] public int FreeCurrency { get; set; }
        [Id(17)] public int PremiumCurrency { get; set; }
        [Id(18)] public int Fame { get; set; }
        [Id(19)] public int Alignment { get; set; }
        [Id(20)] public int Score { get; set; }
        [Id(21)] public int Gender { get; set; }
        [Id(22)] public int XP { get; set; }
        [Id(23)] public int Size { get; set; }
        [Id(24)] public float Weight { get; set; }
    }
}
