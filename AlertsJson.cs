using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pakar_alert_overlay
{
    internal class AlertsJson
    {
        public string? id { get; set; }
        public string? cat { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public string[]? data { get; set; }
    }
}
