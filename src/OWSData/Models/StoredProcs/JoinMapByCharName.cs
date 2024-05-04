using Orleans;

namespace OWSData.Models.StoredProcs
{
    [GenerateSerializer]
    public class JoinMapByCharName
    {
        public JoinMapByCharName()
        {
            NeedToStartupMap = false; //Will get set to true if we can't find a running Zone Instance of zoneName that meets all the required conditions.
            EnableAutoLoopback = false; //No longer using the Customer setting EnableAutoLoopback.  Always false.
            NoPortForwarding = false; //No longer used.  Always false.
        }

        [Id(0)] public string ServerIP { get; set; }
        [Id(1)] public string WorldServerIP { get; set; }
        [Id(2)] public int WorldServerPort { get; set; }
        [Id(3)] public int Port { get; set; }
        [Id(4)] public int MapInstanceID { get; set; }
        [Id(5)] public string MapNameToStart { get; set; }
        [Id(6)] public int WorldServerID { get; set; }
        [Id(7)] public int MapInstanceStatus { get; set; }
        [Id(8)] public bool NeedToStartupMap { get; set; }
        [Id(9)] public bool EnableAutoLoopback { get; set; }
        [Id(10)] public bool NoPortForwarding { get; set; }
        [Id(11)] public bool Success { get; set; }
        [Id(12)] public string ErrorMessage { get; set; }
    }
}
