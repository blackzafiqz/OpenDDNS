using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OpenDDNSLib.Driver.Provider.PowerDns
{
    public class Record
    {
        /// <summary>
        /// The content of this record
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; }

        /// <summary>
        /// Whether or not this record is disabled. When unset, the record is not disabled
        /// </summary>
        [JsonPropertyName ("disabled")]
        public bool? Disabled { get; set; }

    }
}
