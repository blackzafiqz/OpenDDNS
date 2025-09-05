using System.Text.Json.Serialization;

namespace OpenDDNSLib.Driver.Provider.PowerDns
{
    public class RRSet
    {
        /// <summary>
        /// Name for record set (e.g. “www.powerdns.com.”)
        /// </summary>
        [JsonPropertyName("name")]
        [JsonRequired]
        public required string Name { get; set; }

        /// <summary>
        /// Type of this record (e.g. “A”, “PTR”, “MX”)
        /// </summary>
        [JsonPropertyName("type")]
        [JsonRequired]
        public string Type { get; set; }

        /// <summary>
        /// DNS TTL of the records, in seconds. MUST NOT be included when changetype is set to “DELETE”.
        /// </summary>
        [JsonPropertyName("ttl")]
        public int Ttl { get; set; }

        /// <summary>
        /// MUST be added when updating the RRSet. Must be REPLACE or DELETE. With DELETE, all existing RRs matching name and type will be deleted, including all comments. With REPLACE: when records is present, all existing RRs matching name and type will be deleted, and then new records given in records will be created. If no records are left, any existing comments will be deleted as well. When comments is present, all existing comments for the RRs matching name and type will be deleted, and then new comments given in comments will be created.
        /// </summary>
        [JsonPropertyName("changetype")]
        public required string Changetype { get; set; }

        /// <summary>
        /// All records in this RRSet. When updating Records, this is the list of new records (replacing the old ones). Must be empty when changetype is set to DELETE. An empty list results in deletion of all records (and comments).
        /// </summary>
        [JsonPropertyName("records")]
        [JsonRequired]
        public System.Collections.Generic.ICollection<Record> Records { get; set; } = new System.Collections.ObjectModel.Collection<Record>();

        ///// <summary>
        ///// List of Comment. Must be empty when changetype is set to DELETE. An empty list results in deletion of all comments. modified_at is optional and defaults to the current server time.
        ///// </summary>
        //[JsonPropertyName("comments")]
        //public System.Collections.Generic.ICollection<Comment> Comments { get; set; }

    }
}
