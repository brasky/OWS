using Orleans;
using System;

namespace OWSData.Models.StoredProcs
{
    [GenerateSerializer(GenerateFieldIds=GenerateFieldIds.PublicProperties)]
    public class PlayerLoginAndCreateSession
    {
        public bool Authenticated { get; set; }
        public Guid? UserSessionGuid { get; set; }
        public string ErrorMessage { get; set; }

        public PlayerLoginAndCreateSession()
        {
            ErrorMessage = "";
        }
    }
}
