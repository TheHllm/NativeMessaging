using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NativeMessaging
{
    [DebuggerDisplay("{Name}")]
    public class ManifestFile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("allowed_origins")]
        public string[] AllowedOrigins { get; set; }

        public ManifestFile() { }

        public static async Task<ManifestFile> LoadFile(string path)
        {
            string data;
            using (var streamReader = new StreamReader(path))
            {
                await streamReader.ReadToEndAsync();
                data = File.ReadAllText(path);
            }
            return JsonSerializer.Deserialize<ManifestFile>(data);
        }
    }
}
