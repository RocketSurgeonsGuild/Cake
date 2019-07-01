using System.Collections.Generic;
using System.Linq;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Rocket.Surgery.Cake;
using Rocket.Surgery.Cake.Internal;

namespace Rocket.Surgery.Cake.TfsTasks
{
    /// <summary>
    /// Tfs.
    /// </summary>
    public class Tfs
    {
        private readonly ICakeContext _context;
        private readonly JsonSerializerSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tfs"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public Tfs(ICakeContext context)
        {
            _context = context;
            _settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            };
            _settings.Converters.Add(new FilePathConverter());
            _settings.Converters.Add(new DirectoryPathConverter());
            _settings.Converters.Add(new TfsEnumConverter());
        }

        /// <summary>
        /// Uploads the artifacts.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="containerFolder">The container folder.</param>
        /// <param name="localPath">The local path.</param>
        public void UploadArtifacts(string name, string containerFolder, string localPath)
        {
            UploadArtifacts(new UploadArtifactsOptions()
            {
                Name = name,
                ContainerFolder = containerFolder,
                LocalPath = new DirectoryPath(localPath),
            });
        }

        /// <summary>
        /// Uploads the artifacts.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="containerFolder">The container folder.</param>
        /// <param name="localPath">The local path.</param>
        public void UploadArtifacts(string name, string containerFolder, DirectoryPath localPath)
        {
            UploadArtifacts(new UploadArtifactsOptions()
            {
                Name = name,
                ContainerFolder = containerFolder,
                LocalPath = localPath,
            });
        }

        /// <summary>
        /// Uploads the artifacts.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="containerFolder">The container folder.</param>
        /// <param name="localPath">The local path.</param>
        public void UploadArtifacts(string name, string containerFolder, FilePath localPath)
        {
            UploadArtifacts(new UploadArtifactsOptions()
            {
                Name = name,
                ContainerFolder = containerFolder,
                LocalPath = localPath,
            });
        }

        /// <summary>
        /// Uploads the artifacts.
        /// </summary>
        /// <param name="options">The options.</param>
        public void UploadArtifacts(UploadArtifactsOptions options)
        {
            if (options.LocalPath?.IsRelative == true)
            {
                if (options.LocalPath is DirectoryPath dp)
                {
                    options.LocalPath = dp.MakeAbsolute(_context.Environment);
                }

                if (options.LocalPath is FilePath fp)
                {
                    options.LocalPath = fp.MakeAbsolute(_context.Environment);
                }
            }

            WriteLoggingCommand("artifact.upload", options, options.LocalPath.FullPath);
        }

        /// <summary>
        /// Adds the summary item.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="attachmentPath">The attachment path.</param>
        public void AddSummaryItem(string name, FilePath attachmentPath)
        {
            AddAttachment("Distributedtask.Core.Summary", name, attachmentPath);
        }

        /// <summary>
        /// Adds the attachment.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="attachmentPath">The attachment path.</param>
        public void AddAttachment(string type, string name, FilePath attachmentPath)
        {
            WriteLoggingCommand("task.addattachment", new { type, name }, attachmentPath.MakeAbsolute(_context.Environment).FullPath);
        }

        /// <summary>
        /// Reports the code coverage.
        /// </summary>
        /// <param name="summaryFile">The summary file.</param>
        public void ReportCodeCoverage(FilePath summaryFile)
        {
            ReportCodeCoverage(new ReportCodeCoverageOptions()
            {
                SummaryFile = summaryFile
            });
        }

        /// <summary>
        /// Reports the code coverage.
        /// </summary>
        /// <param name="summaryFile">The summary file.</param>
        /// <param name="reportDirectory">The report directory.</param>
        public void ReportCodeCoverage(FilePath summaryFile, DirectoryPath reportDirectory)
        {
            ReportCodeCoverage(new ReportCodeCoverageOptions()
            {
                SummaryFile = summaryFile,
                ReportDirectory = reportDirectory
            });
        }

        /// <summary>
        /// Reports the code coverage.
        /// </summary>
        /// <param name="summaryFile">The summary file.</param>
        /// <param name="reportDirectory">The report directory.</param>
        /// <param name="additionalCodeCoverageFiles">The additional code coverage files.</param>
        public void ReportCodeCoverage(FilePath summaryFile,
            DirectoryPath reportDirectory, string additionalCodeCoverageFiles)
        {
            ReportCodeCoverage(new ReportCodeCoverageOptions()
            {
                SummaryFile = summaryFile,
                ReportDirectory = reportDirectory,
                AdditionalCodeCoverageFiles = _context.GetFiles(additionalCodeCoverageFiles)
            });
        }

        /// <summary>
        /// Reports the code coverage.
        /// </summary>
        /// <param name="summaryFile">The summary file.</param>
        /// <param name="reportDirectory">The report directory.</param>
        /// <param name="additionalCodeCoverageFiles">The additional code coverage files.</param>
        public void ReportCodeCoverage(FilePath summaryFile,
            DirectoryPath reportDirectory, IEnumerable<FilePath> additionalCodeCoverageFiles)
        {
            ReportCodeCoverage(new ReportCodeCoverageOptions()
            {
                SummaryFile = summaryFile,
                ReportDirectory = reportDirectory,
                AdditionalCodeCoverageFiles = additionalCodeCoverageFiles
            });
        }

        /// <summary>
        /// Reports the code coverage.
        /// </summary>
        /// <param name="summaryFile">The summary file.</param>
        /// <param name="reportDirectory">The report directory.</param>
        /// <param name="additionalCodeCoverageFiles">The additional code coverage files.</param>
        public void ReportCodeCoverage(FilePath summaryFile,
            DirectoryPath reportDirectory, IEnumerable<string> additionalCodeCoverageFiles)
        {
            ReportCodeCoverage(new ReportCodeCoverageOptions()
            {
                SummaryFile = summaryFile,
                ReportDirectory = reportDirectory,
                AdditionalCodeCoverageFiles = additionalCodeCoverageFiles.SelectMany(_context.GetFiles)
            });
        }

        /// <summary>
        /// Reports the code coverage.
        /// </summary>
        /// <param name="options">The options.</param>
        public void ReportCodeCoverage(ReportCodeCoverageOptions options)
        {
            if (options.SummaryFile?.IsRelative == true)
            {
                options.SummaryFile = options.SummaryFile.MakeAbsolute(_context.Environment);
            }

            if (options.ReportDirectory?.IsRelative == true)
            {
                options.ReportDirectory = options.ReportDirectory.MakeAbsolute(_context.Environment);
            }

            if (options.AdditionalCodeCoverageFiles?.Any() == true)
            {
                options.AdditionalCodeCoverageFiles = options.AdditionalCodeCoverageFiles
                    .Select(file => file.IsRelative ? file.MakeAbsolute(_context.Environment) : file);
            }

            WriteLoggingCommand("codecoverage.publish", options, "");
        }

        /// <summary>
        /// Reports the test results.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="title">The title.</param>
        /// <param name="files">The files.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="platform">The platform.</param>
        /// <param name="mergeResults">if set to <c>true</c> [merge results].</param>
        /// <param name="publishRunAttachments">if set to <c>true</c> [publish run attachments].</param>
        public void ReportTestResults(TestResultsType type, string title,
            string files, string configuration = null, string platform = null, bool mergeResults = true,
            bool publishRunAttachments = true)
        {
            if (configuration == null) configuration = _context.Configuration();

            ReportTestResults(new ReportTestResultsOptions()
            {
                Type = type,
                Title = title,
                Files = _context.GetFiles(files),
                Configuration = configuration,
                Platform = platform,
                MergeResults = mergeResults,
                PublishRunAttachments = publishRunAttachments,
            });
        }

        /// <summary>
        /// Reports the test results.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="title">The title.</param>
        /// <param name="files">The files.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="platform">The platform.</param>
        /// <param name="mergeResults">if set to <c>true</c> [merge results].</param>
        /// <param name="publishRunAttachments">if set to <c>true</c> [publish run attachments].</param>
        public void ReportTestResults(TestResultsType type, string title,
            IEnumerable<FilePath> files, string configuration = null, string platform = null, bool mergeResults = true,
            bool publishRunAttachments = true)
        {
            if (configuration == null) configuration = _context.Configuration();

            ReportTestResults(new ReportTestResultsOptions()
            {
                Type = type,
                Title = title,
                Files = files,
                Configuration = configuration,
                Platform = platform,
                MergeResults = mergeResults,
                PublishRunAttachments = publishRunAttachments,
            });
        }

        /// <summary>
        /// Reports the test results.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="title">The title.</param>
        /// <param name="files">The files.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="platform">The platform.</param>
        /// <param name="mergeResults">if set to <c>true</c> [merge results].</param>
        /// <param name="publishRunAttachments">if set to <c>true</c> [publish run attachments].</param>
        public void ReportTestResults(TestResultsType type, string title,
            IEnumerable<string> files, string configuration = null, string platform = null, bool mergeResults = true,
            bool publishRunAttachments = true)
        {
            if (configuration == null) configuration = _context.Configuration();

            ReportTestResults(new ReportTestResultsOptions()
            {
                Type = type,
                Title = title,
                Files = files.SelectMany(_context.GetFiles),
                Configuration = configuration,
                Platform = platform,
                MergeResults = mergeResults,
                PublishRunAttachments = publishRunAttachments,
            });
        }

        /// <summary>
        /// Reports the test results.
        /// </summary>
        /// <param name="options">The options.</param>
        public void ReportTestResults(ReportTestResultsOptions options)
        {
            if (options.Files?.Any() == true)
            {
                options.Files = options.Files
                    .Select(file => file.IsRelative ? file.MakeAbsolute(_context.Environment) : file);
            }

            WriteLoggingCommand("results.publish", options, "");
        }

        private const string MessagePrefix = "##vso[";
        private const string MessagePostfix = "]";

        private void WriteLoggingCommand(string actionName, object properties, string value)
        {
            var serializedresult = JsonConvert.SerializeObject(properties, _settings);
            IDictionary<string, JToken> dictionary = JObject.Parse(serializedresult);

            var props = string.Join(string.Empty, dictionary.Select(pair =>
            {
                if (pair.Value is JArray ja)
                {
                    return $"{pair.Key}={string.Join(",", ja)};";
                }
                return $"{pair.Key}={pair.Value};";
            }));

            _context.Log.Write(Verbosity.Quiet, LogLevel.Information, "{0}{1} {2}{3}{4}", MessagePrefix, actionName, props, MessagePostfix, value);
        }
    }
}
