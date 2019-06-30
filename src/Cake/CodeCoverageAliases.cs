using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.DotCover;
using Cake.Common.Tools.DotCover.Cover;
using Cake.Common.Tools.DotCover.Report;
using Cake.Common.Tools.OpenCover;
using Cake.Common.Xml;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using Cake.Core.Tooling;
using Rocket.Surgery.Cake.LcovConverter;

namespace Rocket.Surgery.Cake
{
    /// <summary>
    /// Class CodeCoverageAliases.
    /// </summary>
    public static class CodeCoverageAliases
    {
        /// <summary>
        /// Converts the lcov to cobertura.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="baseDirectory">The base directory.</param>
        /// <param name="lcovFile">The lcov file.</param>
        /// <param name="coberturaFile">The cobertura file.</param>
        [CakeMethodAlias]
        public static void ConvertLcovToCobertura(this ICakeContext context, DirectoryPath baseDirectory, FilePath lcovFile, FilePath coberturaFile)
        {
            var converter = new LcovToCoberturaConverter(context, baseDirectory);
            var document = converter.Convert(File.ReadAllText(lcovFile.MakeAbsolute(context.Environment).FullPath));
            document.Save(context.FileSystem.GetFile(coberturaFile.MakeAbsolute(context.Environment)).OpenWrite());
        }

        /// <summary>
        /// Dots the cover to cobertura summary.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="xmlFile">The XML file.</param>
        /// <param name="coberturaFile">The cobertura file.</param>
        [CakeMethodAlias]
        public static void DotCoverToCoberturaSummary(this ICakeContext context, FilePath xmlFile, FilePath coberturaFile)
        {
            // context.Warning("TODO: ADD COBERTURA");

            // Name constants
            var linesValid = XName.Get("lines-valid");
            var linesCovered = XName.Get("lines-covered");
            var lineRate = XName.Get("line-rate");
            //var branchesValid = XName.Get("branches-valid");
            //var branchesCovered = XName.Get("branches-covered");
            //var branchRate = XName.Get("branch-rate");
            var timestamp = XName.Get("timestamp");
            var complexity = XName.Get("complexity");
            var version = XName.Get("version");

            // Create a new coverage element
            var newCoverage = new XElement(XName.Get("coverage"));

            // Compute the new values for the coverage attributes
            var longLinesValid = long.Parse(context.XmlPeek(xmlFile, "/Root/@TotalStatements"));
            if (longLinesValid <= 0) longLinesValid = 1;
            var longLinesCovered = long.Parse(context.XmlPeek(xmlFile, "/Root/@CoveredStatements"));
            var decimalLineRate = ((decimal)longLinesCovered) / longLinesValid;

            // Set them on the new coverage
            newCoverage.SetAttributeValue(timestamp, DateTimeOffset.Now.ToUnixTimeSeconds());
            newCoverage.SetAttributeValue(version, "0.1");
            newCoverage.SetAttributeValue(linesValid, longLinesValid);
            newCoverage.SetAttributeValue(linesCovered, longLinesCovered);
            newCoverage.SetAttributeValue(lineRate, decimalLineRate);
            //newCoverage.SetAttributeValue(branchesValid, 0);
            //newCoverage.SetAttributeValue(branchesCovered, 0);
            //newCoverage.SetAttributeValue(branchRate, 0);
            newCoverage.SetAttributeValue(complexity, "0");

            // Add the default sources
            newCoverage.Add(XElement.Parse(@"<sources><source>.</source></sources>"));
            var newDoc = new XDocument(new XDeclaration("1.0", null, null), new XDocumentType("coverage", null, "http://cobertura.sourceforge.net/xml/coverage-04.dtd", null), newCoverage);
            newDoc.Save(context.FileSystem.GetFile(coberturaFile).OpenWrite());
        }

        /// <summary>
        /// Merges the cobertura files.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="files">The files.</param>
        /// <param name="outputFile">The output file.</param>
        [CakeMethodAlias]
        public static void MergeCoberturaFiles(this ICakeContext context, string files, FilePath outputFile)
        {
            MergeCoberturaFiles(context, context.GetFiles(files), outputFile);
        }

        /// <summary>
        /// Merges the cobertura files.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="files">The files.</param>
        /// <param name="outputFile">The output file.</param>
        [CakeMethodAlias]
        public static void MergeCoberturaFiles(this ICakeContext context, IEnumerable<FilePath> files, FilePath outputFile)
        {
            // Load all the reports
            var xdocs = files.Select(x => XDocument.Load((string)x.FullPath)).ToArray();

            // Normalize the sources
            // in some cases the file paths are relative, this will normalize
            // them so they will all be absolute
            foreach (var result in xdocs.Descendants(XName.Get("sources"))
                .Select(x => x.Descendants(XName.Get("source")).FirstOrDefault())
                .Where(x => x != null && x.Value != ".")
                .Select(x => new { Source = x.Value, Document = x.Ancestors().Last() }))
            {
                foreach (var @class in result.Document.Descendants(XName.Get("class")))
                {
                    @class.SetAttributeValue(XName.Get("filename"), System.IO.Path.Combine(result.Source, @class.Attribute(XName.Get("filename")).Value));
                }
            }

            // Name constants
            var linesValid = XName.Get("lines-valid");
            var linesCovered = XName.Get("lines-covered");
            var lineRate = XName.Get("line-rate");
            var branchesValid = XName.Get("branches-valid");
            var branchesCovered = XName.Get("branches-covered");
            var branchRate = XName.Get("branch-rate");
            var timestamp = XName.Get("timestamp");
            var complexity = XName.Get("complexity");
            var version = XName.Get("version");

            // Find all the coverage elements
            var coverages = xdocs
                .Descendants(XName.Get("coverage"))
                .Select(x => x.Attributes().ToDictionary(z => z.Name, z => (string)z.Value))
                .ToArray();

            // Create a new coverage element
            var newCoverage = new XElement(XName.Get("coverage"));

            // Compute the new values for the coverage attributes
            var longLinesValid = coverages.Sum(x => long.Parse(x[linesValid]));
            if (longLinesValid <= 0) longLinesValid = 1;
            var longLinesCovered = coverages.Sum(x => long.Parse(x[linesCovered]));
            var longBranchesValid = coverages.Sum(x => long.Parse(x[branchesValid]));
            if (longBranchesValid <= 0) longBranchesValid = 1;
            var longBranchesCovered = coverages.Sum(x => long.Parse(x[branchesCovered]));
            var decimalLineRate = ((decimal)longLinesCovered) / longLinesValid;
            var decimalBranchRate = ((decimal)longBranchesCovered) / longBranchesValid;

            // Set them on the new coverage
            try { newCoverage.SetAttributeValue(timestamp, coverages.Max(x => x[timestamp])); } catch { }
            try { newCoverage.SetAttributeValue(version, coverages.Max(x => x[version])); } catch { }
            newCoverage.SetAttributeValue(linesValid, longLinesValid);
            newCoverage.SetAttributeValue(linesCovered, longLinesCovered);
            newCoverage.SetAttributeValue(branchesValid, longBranchesValid);
            newCoverage.SetAttributeValue(branchesCovered, longBranchesCovered);
            newCoverage.SetAttributeValue(lineRate, decimalLineRate);
            newCoverage.SetAttributeValue(branchRate, decimalBranchRate);

            // Add the default sources
            newCoverage.Add(XElement.Parse(@"<sources><source>.</source></sources>"));
            // Create a new packages element
            var newPackages = new XElement(XName.Get("packages"));
            // Get all the package elements from all documents
            var packages = xdocs
                .Descendants(XName.Get("package"));

            newPackages.Add(packages);
            newCoverage.Add(newPackages);
            var newDoc = new XDocument(new XDeclaration(xdocs.First().Declaration), xdocs.First().DocumentType, newCoverage);
            newDoc.Save(context.FileSystem.GetFile(outputFile).OpenWrite());
        }

        /// <summary>
        /// Dots the cover cobertura.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="inputProject">The input project.</param>
        /// <param name="outputDirectory">The output directory.</param>
        /// <param name="tool">The tool.</param>
        /// <param name="settings">The settings.</param>
        [CakeMethodAlias]
        [CakeNamespaceImport("Cake.Common.Tools.DotCover.Cover")]
        public static void DotCoverCobertura(this ICakeContext context, FilePath inputProject, DirectoryPath outputDirectory, Action<ICakeContext> tool, DotCoverCoverSettings settings)
        {
            context.EnsureDirectoryExists(outputDirectory);

            var reportPath = outputDirectory.MakeAbsolute(context.Environment).CombineWithFilePath(inputProject.GetFilename().ChangeExtension("dcvr"));

            if (settings.TargetWorkingDir == null)
                settings.TargetWorkingDir = inputProject.GetDirectory();

            context.DotCoverCover(tool, reportPath, settings);
        }

        /// <summary>
        /// Dots the cover reports.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="inputFiles">The input files.</param>
        /// <param name="outputDirectory">The output directory.</param>
        /// <param name="reportTypes">The report types.</param>
        [CakeMethodAlias]
        [CakeNamespaceImport("Rocket.Surgery.Cake")]
        [CakeNamespaceImport("Cake.Common.Tools.DotCover")]
        public static void DotCoverReports(this ICakeContext context, string inputFiles, DirectoryPath outputDirectory, params DotCoverReportType[] reportTypes)
        {
            DotCoverReports(context, context.GetFiles(inputFiles), new DotCoverReportsSettings
            {
                OutputDirectory = outputDirectory,
                ReportTypes = reportTypes
            });
        }

        /// <summary>
        /// Dots the cover reports.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="inputFiles">The input files.</param>
        /// <param name="outputDirectory">The output directory.</param>
        /// <param name="reportTypes">The report types.</param>
        [CakeMethodAlias]
        [CakeNamespaceImport("Rocket.Surgery.Cake")]
        [CakeNamespaceImport("Cake.Common.Tools.DotCover")]
        public static void DotCoverReports(this ICakeContext context, IEnumerable<FilePath> inputFiles, DirectoryPath outputDirectory, params DotCoverReportType[] reportTypes)
        {
            DotCoverReports(context, inputFiles, new DotCoverReportsSettings
            {
                OutputDirectory = outputDirectory,
                ReportTypes = reportTypes
            });
        }

        /// <summary>
        /// Dots the cover reports.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="inputFiles">The input files.</param>
        /// <param name="settings">The settings.</param>
        [CakeMethodAlias]
        [CakeNamespaceImport("Rocket.Surgery.Cake")]
        [CakeNamespaceImport("Cake.Common.Tools.DotCover")]
        public static void DotCoverReports(this ICakeContext context, IEnumerable<FilePath> inputFiles, DotCoverReportsSettings settings)
        {
            context.EnsureDirectoryExists(settings.OutputDirectory);
            var coverageReport = settings.OutputDirectory.CombineWithFilePath(settings.ReportName);

            try
            {
                context.DotCoverMerge(inputFiles, coverageReport);
            }
            catch
            {
                // Sometimes we come in so ho that dotcover is still churning
                Task.Delay(500).Wait();
                context.DotCoverMerge(inputFiles, coverageReport);
            }

            foreach (var type in settings.ReportTypes)
            {
                context.DotCoverReport(
                    coverageReport,
                    coverageReport.ChangeExtension(type.ToString().ToLower()),
                    new DotCoverReportSettings()
                    {
                        ReportType = type
                    }
                );
            }

            DotCoverToCoberturaSummary(
                context,
                coverageReport.ChangeExtension("xml"),
                coverageReport.ChangeExtension("cobertura"));
        }
    }

    /// <summary>
    /// Class DotCoverReportsSettings.
    /// Implements the <see cref="Cake.Core.Tooling.ToolSettings" />
    /// </summary>
    /// <seealso cref="Cake.Core.Tooling.ToolSettings" />
    public class DotCoverReportsSettings : ToolSettings
    {
        /// <summary>
        /// Gets or sets the output directory.
        /// </summary>
        /// <value>The output directory.</value>
        public DirectoryPath OutputDirectory { get; set; }
        /// <summary>
        /// Gets or sets the name of the report.
        /// </summary>
        /// <value>The name of the report.</value>
        public string ReportName { get; set; } = "solution.dcvr";
        private IEnumerable<DotCoverReportType> _reportTypes = Array.Empty<DotCoverReportType>();
        /// <summary>
        /// Gets or sets the report types.
        /// </summary>
        /// <value>The report types.</value>
        public IEnumerable<DotCoverReportType> ReportTypes
        {
            get => _reportTypes.Concat(new[] { DotCoverReportType.XML }).Distinct();
            set => _reportTypes = value;
        }
    }
}
