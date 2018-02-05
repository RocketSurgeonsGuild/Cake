using System.Collections.Generic;
using System.Linq;
using Cake.Core.IO;
using Newtonsoft.Json;
using Rocket.Surgery.Cake.Internal;

namespace Rocket.Surgery.Cake.TfsTasks
{
    public class ReportCodeCoverageOptions
    {
        [JsonProperty("codecoveragetool")]
        public CodeCoverageType CodeCoverageType { get; } = CodeCoverageType.Cobertura;

        [JsonProperty("summaryfile")]
        public FilePath SummaryFile { get; set; }

        [JsonProperty("reportdirectory")]
        public DirectoryPath ReportDirectory { get; set; }

        [JsonProperty("additionalcodecoveragefiles")]
        public IEnumerable<FilePath> AdditionalCodeCoverageFiles { get; set; }
    }
}
