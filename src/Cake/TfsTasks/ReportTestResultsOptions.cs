using System.Collections.Generic;
using Cake.Core.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rocket.Surgery.Cake.TfsTasks
{
    /// <summary>
    /// Class ReportTestResultsOptions.
    /// </summary>
    public class ReportTestResultsOptions
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public TestResultsType Type { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        [JsonProperty("resultFiles")]
        public IEnumerable<FilePath> Files { get; set; }
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public string Configuration { get; set; }
        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        /// <value>The platform.</value>
        public string Platform { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [merge results].
        /// </summary>
        /// <value><c>true</c> if [merge results]; otherwise, <c>false</c>.</value>
        public bool MergeResults { get; set; } = true;
        /// <summary>
        /// Gets or sets a value indicating whether [publish run attachments].
        /// </summary>
        /// <value><c>true</c> if [publish run attachments]; otherwise, <c>false</c>.</value>
        public bool PublishRunAttachments { get; set; } = true;
    }
}
