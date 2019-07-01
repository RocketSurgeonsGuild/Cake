using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.IO;
using Path = System.IO.Path;

namespace Rocket.Surgery.Cake.LcovConverter
{
    /// <summary>
    /// LcovToCoberturaConverter.
    /// </summary>
    class LcovToCoberturaConverter
    {
        private readonly ICakeContext _context;
        private readonly DirectoryPath _baseDirectory;
        /// <summary>
        /// Initializes a new instance of the <see cref="LcovToCoberturaConverter"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="baseDirectory">The base directory.</param>
        public LcovToCoberturaConverter(ICakeContext context, DirectoryPath baseDirectory)
        {
            _context = context;
            _baseDirectory = baseDirectory.MakeAbsolute(context.Environment);
        }

        /// <summary>
        /// Converts the specified lcov data.
        /// </summary>
        /// <param name="lcovData">The lcov data.</param>
        /// <returns>XDocument.</returns>
        public XDocument Convert(string lcovData)
        {
            return GenerateCoberturaXml(GetCoverageData(lcovData));
        }

        private CoverageData GetCoverageData(string lcovData)
        {
            var coverageData = new CoverageData();
            string currentPackage = null;
            string currentFile = null;

            long fileLinesTotal = 0;
            long fileLinesCovered = 0;
            long fileBranchesTotal = 0;
            long fileBranchesCovered = 0;

            var fileLines = new Dictionary<long, FileLine>();
            var fileMethods = new Dictionary<string, FileMethod>();

            foreach (var line in lcovData.Split('\n'))
            {
                if (line.Trim() == "end_of_record" && currentFile != null)
                {
                    // new { currentPackage, currentFile }.Dump();
                    var package = coverageData.Packages[currentPackage];
                    package.LinesTotal += fileLinesTotal;
                    package.LinesCovered += fileLinesCovered;
                    package.BranchesTotal += fileBranchesTotal;
                    package.BranchesCovered += fileBranchesCovered;

                    var file = package.Classes[currentFile];
                    file.LinesTotal = fileLinesTotal;
                    file.LinesCovered = fileLinesCovered;
                    file.BranchesTotal = fileBranchesTotal;
                    file.BranchesCovered = fileBranchesCovered;
                    foreach (var item in fileLines)
                        file.Lines.Add(item);
                    foreach (var item in fileMethods)
                        file.Methods.Add(item);

                    coverageData.LinesTotal += fileLinesTotal;
                    coverageData.LinesCovered += fileLinesCovered;
                    coverageData.BranchesTotal += fileBranchesTotal;
                    coverageData.BranchesCovered += fileBranchesCovered;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line)) continue;

                // line.Dump();
                var inputType = line.Substring(0, line.IndexOf(":"));
                var lineContent = line.Substring(line.IndexOf(":") + 1);
                var parts = lineContent.Trim().Split(',');

                switch (inputType)
                {
                    case "SF":
                        var fileName = lineContent.Trim();
                        // TODO: USE cake method here...
                        var relativeFileName = _baseDirectory.GetRelativePath(new FilePath(fileName));
                        var packageName = string.Join(".", relativeFileName.Segments.Take(relativeFileName.Segments.Length - 1));
                        var className = relativeFileName.Segments.Last();

                        if (!coverageData.Packages.TryGetValue(packageName, out var package))
                        {
                            package = new Package();
                            coverageData.Packages.Add(packageName, package);
                        }

                        package.Classes[relativeFileName.FullPath] = new Class(className);

                        currentPackage = packageName;
                        currentFile = relativeFileName.FullPath;
                        fileLinesTotal = 0;
                        fileLinesCovered = 0;
                        fileBranchesTotal = 0;
                        fileBranchesCovered = 0;
                        fileLines = new Dictionary<long, FileLine>();
                        fileMethods = new Dictionary<string, FileMethod>();
                        break;

                    case "DA":
                        // DA:2,0
                        {
                            var lineNumber = long.Parse(parts[0]);
                            var lineHits = parts[1];

                            if (!fileLines.TryGetValue(lineNumber, out var fileLine))
                            {
                                fileLine = new FileLine();
                                fileLines.Add(lineNumber, fileLine);
                            }

                            if (long.TryParse(lineHits, out var hits))
                            {
                                fileLine.Hits += hits;
                                if (hits > 0)
                                {
                                    fileLinesCovered++;
                                }
                            }
                            fileLinesTotal++;
                        }
                        break;

                    case "BRDA":
                        // BRDA:1,1,2,0
                        {
                            var lineNumber = long.Parse(parts[0]);
                            var blockNumber = long.Parse(parts[1]);
                            var branchNumber = long.Parse(parts[2]);
                            var branchHits = parts[3];

                            if (!fileLines.TryGetValue(lineNumber, out var fileLine))
                            {
                                fileLine = new FileLine();
                                fileLines.Add(lineNumber, fileLine);
                            }

                            fileLine.Branch = true;
                            fileLine.BranchesTotal++;
                            fileBranchesTotal++;

                            var covered = false;

                            if (branchHits != "-" && long.Parse(branchHits) > 0)
                            {
                                covered = true;
                                fileLine.BranchesCovered++;
                                fileBranchesCovered++;
                            }
                            fileLine.Conditions.Add(new BranchCondition()
                            {
                                Number = branchNumber,
                                Covered = covered
                            });
                        }
                        break;

                    case "BRF":
                        {
                            fileBranchesTotal = long.Parse(lineContent);
                        }
                        break;

                    case "BRH":
                        {
                            fileBranchesCovered = long.Parse(lineContent);
                        }
                        break;

                    case "FN":
                        {
                            // FN:5,(anonymous_1)

                            var functionLine = long.Parse(parts[0]);
                            var functionName = parts[1];

                            fileMethods[functionName] = new FileMethod()
                            {
                                Line = functionLine,
                                Hits = 0
                            };
                        }
                        break;

                    case "FNDA":
                        {
                            // FNDA:0,(anonymous_1)
                            var functionHits = long.Parse(parts[0]);
                            var functionName = parts[1];

                            if (!fileMethods.TryGetValue(functionName, out var fileMethod))
                            {
                                fileMethod = new FileMethod() { Line = 0, Hits = 0 };
                                fileMethods.Add(functionName, fileMethod);
                            }

                            fileMethod.Hits = functionHits;
                        }
                        break;
                }
            }

            return coverageData;
        }

        private XDocument GenerateCoberturaXml(CoverageData data)
        {
            var doc = new XDocument();
            doc.Add(new XDocumentType("coverage", "SYSTEM", "http://cobertura.sourceforge.net/xml/coverage-04.dtd", ""));
            doc.Declaration = new XDeclaration("1.0", null, null);
            var root = new XElement("coverage");
            doc.Add(root);

            root.SetAttributeValue("branch-rate", data.BranchRate);
            root.SetAttributeValue("branches-covered", data.BranchesCovered);
            root.SetAttributeValue("branches-valid", data.BranchesTotal);
            root.SetAttributeValue("complexity", "0");
            root.SetAttributeValue("line-rate", data.LineRate);
            root.SetAttributeValue("lines-covered", data.LinesCovered);
            root.SetAttributeValue("lines-valid", data.LinesTotal);
            root.SetAttributeValue("timestamp", data.Timestamp);
            root.SetAttributeValue("version", "2.0.3");

            var sources = new XElement("sources");
            root.Add(sources);
            var source = new XElement("source");
            sources.Add(source);

            source.Add(new XText(_baseDirectory.FullPath));

            var packagesElement = new XElement("packages");
            root.Add(packagesElement);

            foreach (var package in data.Packages)
            {
                var packageElement = new XElement("package");
                packagesElement.Add(packageElement);

                packageElement.SetAttributeValue("line-rate", package.Value.LineRate);
                packageElement.SetAttributeValue("branch-rate", package.Value.BranchRate);
                packageElement.SetAttributeValue("name", package.Key);
                packageElement.SetAttributeValue("complexity", "0");

                var classesElement = new XElement("classes");
                packageElement.Add(classesElement);

                foreach (var @class in package.Value.Classes)
                {
                    var classElement = new XElement("class");
                    classesElement.Add(classElement);

                    classElement.SetAttributeValue("complexity", "0");
                    classElement.SetAttributeValue("line-rate", @class.Value.LineRate);
                    classElement.SetAttributeValue("branch-rate", @class.Value.BranchRate);
                    classElement.SetAttributeValue("filename", @class.Key);
                    classElement.SetAttributeValue("name", @class.Value.Name);

                    var methodsElement = new XElement("methods");
                    classElement.Add(methodsElement);

                    foreach (var method in @class.Value.Methods)
                    {
                        var methodElement = new XElement("method");
                        methodsElement.Add(methodElement);

                        methodElement.SetAttributeValue("name", method.Key);
                        methodElement.SetAttributeValue("signature", "");
                        methodElement.SetAttributeValue("line-rate", method.Value.Hits > 0 ? "1.0" : "0.0");
                        methodElement.SetAttributeValue("branch-rate", method.Value.Hits > 0 ? "1.0" : "0.0");

                        var linesElement = new XElement("lines");
                        methodElement.Add(linesElement);

                        var lineElement = new XElement("line");
                        linesElement.Add(lineElement);

                        lineElement.SetAttributeValue("hits", method.Value.Hits);
                        lineElement.SetAttributeValue("number", method.Value.Line);
                        lineElement.SetAttributeValue("branch", "false");
                    }

                    {
                        var linesElement = new XElement("lines");
                        classElement.Add(linesElement);

                        foreach (var line in @class.Value.Lines.OrderBy(x => x.Key))
                        {
                            var lineElement = new XElement("line");
                            linesElement.Add(lineElement);
                            lineElement.SetAttributeValue("hits", line.Value.Hits);
                            lineElement.SetAttributeValue("number", line.Key);
                            lineElement.SetAttributeValue("branch", line.Value.Branch);
                            if (line.Value.Branch)
                            {
                                lineElement.SetAttributeValue("condition-coverage",
                                    $"{line.Value.BranchRate * 100}% ({line.Value.BranchesCovered}/{line.Value.BranchesTotal})");

                                var conditionsElement = new XElement("conditions");
                                lineElement.Add(conditionsElement);

                                foreach (var condition in line.Value.Conditions)
                                {
                                    var conditionElement = new XElement("condition");
                                    conditionsElement.Add(conditionElement);

                                    conditionElement.SetAttributeValue("type", condition.Type);
                                    conditionElement.SetAttributeValue("number", condition.Number);
                                    if (condition.Covered)
                                    {
                                        var coverage = Math.Floor(1d / line.Value.Conditions.Count * 100);
                                        conditionElement.SetAttributeValue("coverage", $"{coverage}%");
                                    }
                                    else
                                    {
                                        conditionElement.SetAttributeValue("coverage", "0%");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return doc;
        }
    }
}
