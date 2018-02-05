using Cake.Core.IO;
using Newtonsoft.Json;
using Rocket.Surgery.Cake.Internal;

namespace Rocket.Surgery.Cake.TfsTasks
{
    public class UploadArtifactsOptions
    {
        [JsonProperty("artifacttype")]
        public ArtifactType Type { get; } = ArtifactType.Container;

        [JsonProperty("artifactname")]
        public string Name { get; set; }

        [JsonProperty("containerfolder")]
        public string ContainerFolder { get; set; }

        [JsonIgnore]
        public Path LocalPath { get; set; }
    }

}
