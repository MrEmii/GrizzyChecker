using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace  MinecraftSharp
{
    public partial class Configuration
    {
        [JsonProperty("license")]
        public string License { get; set; }
        
        [JsonProperty("proxy-type")]
        public string ProxyType { get; set; }

        [JsonProperty("auto-update")]
        public bool AutoUpdate { get; set; }

        [JsonProperty("update-server")]
        public string UpdateServer { get; set; }

        [JsonProperty("fail-print")]
        public bool FailPrint { get; set; }

        [JsonProperty("hits-print")]
        public bool HitsPrint { get; set; }

        [JsonProperty("save-hits")]
        public bool SaveHits { get; set; }

        [JsonProperty("save-fails")]
        public bool SaveFails { get; set; }
        [JsonProperty("background-process")]
        public bool Background { get; set; }

        [JsonProperty("formats")]
        public Formats Formats { get; set; }
    }

    public partial class Formats
    {
        [JsonProperty("file-format")]
        public string FileFormat { get; set; }

        [JsonProperty("sfa-format")]
        public string SfaFormat { get; set; }

        [JsonProperty("nfa-format")]
        public string NfaFormat { get; set; }

        [JsonProperty("fa-format")]
        public string FaFormat { get; set; }

        [JsonProperty("migrate-format")]
        public string MigrateFormat { get; set; }
    }

    public partial class Configuration
    {
        public static Configuration FromJson(string json) => JsonConvert.DeserializeObject<Configuration>(json,  Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Configuration self) => JsonConvert.SerializeObject(self,  Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}