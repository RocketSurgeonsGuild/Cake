using System.Collections.Generic;
using System.Linq;
using Cake.Core.IO;
using Newtonsoft.Json;
using Rocket.Surgery.Cake.Internal;

namespace Rocket.Surgery.Cake.TfsTasks
{
    /// <summary>
    /// ReportCodeCoverageOptions.
    /// </summary>
    public class ReportCodeCoverageOptions
    {
        /// <summary>
        /// Gets the type of the code coverage.
        /// </summary>
        /// <value>The type of the code coverage.</value>
        [JsonProperty("codecoveragetool")]
        public CodeCoverageType CodeCoverageType { get; } = CodeCoverageType.Cobertura;

        /// <summary>
        /// Gets or sets the summary file.
        /// </summary>
        /// <value>The summary file.</value>
        [JsonProperty("summaryfile")]
        public FilePath SummaryFile { get; set; }

        /// <summary>
        /// Gets or sets the report directory.
        /// </summary>
        /// <value>The report directory.</value>
        [JsonProperty("reportdirectory")]
        public DirectoryPath ReportDirectory { get; set; }

        /// <summary>
        /// Gets or sets the additional code coverage files.
        /// </summary>
        /// <value>The additional code coverage files.</value>
        [JsonProperty("additionalcodecoveragefiles")]
        public IEnumerable<FilePath> AdditionalCodeCoverageFiles { get; set; }
    }
}
