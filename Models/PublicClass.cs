using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BPMAPI.Models
{
    public partial class UserConnection
    {
        public string DatabaseName { get; set; }
        public string ComputerName { get; set; }
        public string LicenceKey { get; set; }
        public string StructDb { get; set; }
        public Nullable<System.DateTime> TimeEx { get; set; }
        public int stt { get; set; }
    }
}