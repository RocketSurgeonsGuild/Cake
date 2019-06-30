using Cake.Core.IO;
using Newtonsoft.Json;
using Rocket.Surgery.Cake.Internal;

namespace Rocket.Surgery.Cake.TfsTasks
{
    /// <summary>
    /// Class UploadArtifactsOptions.
    /// </summary>
    public class UploadArtifactsOptions
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        [JsonProperty("artifacttype")]
        public ArtifactType Type { get; } = ArtifactType.Container;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [JsonProperty("artifactname")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the container folder.
        /// </summary>
        /// <value>The container folder.</value>
        [JsonProperty("containerfolder")]
        public string ContainerFolder { get; set; }

        /// <summary>
        /// Gets or sets the local path.
        /// </summary>
        /// <value>The local path.</value>
        [JsonIgnore]
        public Path LocalPath { get; set; }
    }

}
