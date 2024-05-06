using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OWSData.Models.Tables;
using Orleans;

namespace OWSData.Models.StoredProcs
{

    public class GetUserSessionComposite
    {
        public UserSessions userSession { get; set; }
        public User user { get; set; }
        public Characters character { get; set; }

    }

    [GenerateSerializer(GenerateFieldIds = GenerateFieldIds.PublicProperties)]
    public class GetUserSession
    {
        [Id(0)] public Guid CustomerGuid { get; set; }
        [Id(1)] public Guid? UserGuid { get; set; }
        [Id(2)] public Guid UserSessionGUID { get; set; }
        [Id(3)] public DateTime LoginDate { get; set; }
        [Id(4)] public string SelectedCharacterName { get; set; }
        [Id(5)] public string FirstName { get; set; }
        [Id(6)] public string LastName { get; set; }
        [Id(7)] public string Email { get; set; }
        [Id(8)] public DateTime CreateDate { get; set; }
        [Id(9)] public DateTime LastAccess { get; set; }
        [Id(10)] public string Role { get; set; }

        [Id(11)] public int CharacterID { get; set; }
        [Id(12)] public string CharName { get; set; }
        [Id(13)] public string ZoneName { get; set; }
        [Id(14)] public double X { get; set; }
        [Id(15)] public double Y { get; set; }
        [Id(16)] public double Z { get; set; }
        [Id(17)] public double Rx { get; set; }
        [Id(18)] public double Ry { get; set; }
        [Id(19)] public double Rz { get; set; }        
    }
}
