using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OWSShared.RequestPayloads
{
    [GenerateSerializer(GenerateFieldIds = GenerateFieldIds.PublicProperties)]
    public class RegisterInstanceLauncherRequestPayload
    {
        public string launcherGUID { get; set; }
        public string ServerIP { get; set; }
        public int MaxNumberOfInstances { get; set; }
        public string InternalServerIP { get; set; }
        public int StartingInstancePort { get; set; }
    }
}