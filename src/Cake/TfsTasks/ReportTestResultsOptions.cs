using System.Collections.Generic;
using Cake.Core.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rocket.Surgery.Cake.TfsTasks
{
    public class ReportTestResultsOptions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TestResultsType Type { get; set; }
        public string Title { get; set; }
        [JsonProperty("resultFiles")]
        public IEnumerable<FilePath> Files { get; set; }
        public string Configuration { get; set; }
        public string Platform { get; set; }
        public bool MergeResults { get; set; } = true;
        public bool PublishRunAttachments { get; set; } = true;
    }
}
