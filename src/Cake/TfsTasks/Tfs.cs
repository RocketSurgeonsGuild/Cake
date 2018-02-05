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
    public class Tfs
    {
        private readonly ICakeContext _context;
        private readonly JsonSerializerSettings _settings;

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

        public void UploadArtifacts(string name, string containerFolder, string localPath)
        {
            UploadArtifacts(new UploadArtifactsOptions()
            {
                Name = name,
                ContainerFolder = containerFolder,
                LocalPath = new DirectoryPath(localPath),
            });
        }

        public void UploadArtifacts(string name, string containerFolder, DirectoryPath localPath)
        {
            UploadArtifacts(new UploadArtifactsOptions()
            {
                Name = name,
                ContainerFolder = containerFolder,
                LocalPath = localPath,
            });
        }

        public void UploadArtifacts(string name, string containerFolder, FilePath localPath)
        {
            UploadArtifacts(new UploadArtifactsOptions()
            {
                Name = name,
                ContainerFolder = containerFolder,
                LocalPath = localPath,
            });
        }

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

        public void AddSummaryItem(string name, FilePath attachmentPath)
        {
            AddAttachment("Distributedtask.Core.Summary", name, attachmentPath);
        }

        public void AddAttachment(string type, string name, FilePath attachmentPath)
        {
            WriteLoggingCommand("task.addattachment", new { type, name }, attachmentPath.MakeAbsolute(_context.Environment).FullPath);
        }

        public void ReportCodeCoverage(FilePath summaryFile)
        {
            ReportCodeCoverage(new ReportCodeCoverageOptions()
            {
                SummaryFile = summaryFile
            });
        }

        public void ReportCodeCoverage(FilePath summaryFile, DirectoryPath reportDirectory)
        {
            ReportCodeCoverage(new ReportCodeCoverageOptions()
            {
                SummaryFile = summaryFile,
                ReportDirectory = reportDirectory
            });
        }

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
