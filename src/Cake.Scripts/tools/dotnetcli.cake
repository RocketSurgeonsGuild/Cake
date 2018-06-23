#addin "nuget:?package=Rocket.Surgery.Cake&version={version}"
#tool "nuget:?package=JetBrains.dotCover.CommandLineTools"

MSBuildSettings GoBuild(string target)
{
    return new MSBuildSettings {
        Targets = { target },
        EnvironmentVariables = Settings.Environment,
        Configuration = Configuration,
        DetailedSummary = false,
        Verbosity = Verbosity.Minimal,
        FileLoggers = {
            new MSBuildFileLogger {
                AppendToLogFile = false,
                LogFile = Artifact($"logs/{target.ToLower()}.log"),
                ShowTimestamp = true,
                Verbosity = Verbosity.Diagnostic,
                PerformanceSummaryEnabled = true,
                SummaryDisabled = false,
            }
        },
        BinaryLogger = new MSBuildBinaryLogSettings {
            Enabled = true,
            FileName = Artifact($"logs/{target.ToLower()}.binlog"),
            Imports = BuildSystem.IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed,
        }
    };
}

Task("dotnet");

Task("dotnet restore")
    .IsDependeeOf("dotnet")
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        MSBuild(solution, GoBuild("Restore").SetVerbosity(Verbosity.Minimal));
    });

Task("dotnet build")
    .IsDependeeOf("dotnet")
    .IsDependentOn("dotnet restore")
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        MSBuild(solution, GoBuild("Build").SetVerbosity(Verbosity.Minimal));
    });

Task("dotnet test")
    .WithCriteria(IsRunningOnUnix)
    .WithCriteria(() => Settings.XUnit.Enabled)
    .IsDependeeOf("dotnet")
    .IsDependentOn("dotnet build")
    .Does(() => {
        EnsureDirectoryExists(Artifact("test"));
    })
    .DoesForEach(
        GetFiles("test/*/*.csproj"), (file) => {
            var unitTestReport = ArtifactFilePath($"test/{file.GetFilenameWithoutExtension().ToString()}.xml")
                .MakeAbsolute(Context.Environment).FullPath;

            DotNetCoreTest(file.FullPath, new DotNetCoreTestSettings() {
                Configuration = Configuration,
                DiagnosticOutput = Settings.XUnit.Detailed,
                NoBuild = !Settings.XUnit.Build,
                NoRestore = !Settings.XUnit.Restore,
                TestAdapterPath = ".",
                Logger = $"\"xunit;LogFilePath={unitTestReport}\""
            });
        })
        .Finally(() => {
            if (!GetFiles("test/*/*.csproj").Any()) return;
        });


Task("dotnet test w/coverage")
    .WithCriteria(IsRunningOnWindows)
    .WithCriteria(() => Settings.XUnit.Enabled)
    .IsDependeeOf("dotnet")
    .IsDependentOn("dotnet build")
    .Does(() => {
        EnsureDirectoryExists(Artifact("test"));
        EnsureDirectoryExists(Coverage);
        EnsureDirectoryExists(CoverageDirectoryPath("report"));
        EnsureDirectoryExists("xunit");
    })
    .DoesForEach(
        GetFiles("test/*/*.csproj"),
        (file) => {
            var unitTestReport = ArtifactFilePath($"test/{file.GetFilenameWithoutExtension().ToString()}.xml")
                .MakeAbsolute(Context.Environment).FullPath;

            DotCoverCover(tool => {
                tool.DotNetCoreTest(file.FullPath, new DotNetCoreTestSettings() {
                    Configuration = Configuration,
                    DiagnosticOutput = Settings.XUnit.Detailed,
                    NoBuild = !Settings.XUnit.Build,
                    NoRestore = !Settings.XUnit.Restore,
                    TestAdapterPath = ".",
                    Logger = $"\"xunit;LogFilePath={unitTestReport}\""
                });
                },
                CoverageFilePath($"{file.GetFilenameWithoutExtension().ToString()}.dcvr").MakeAbsolute(Context.Environment),
                Settings.Coverage.Apply(new DotCoverCoverSettings() {
                    TargetWorkingDir = file.GetDirectory(),
                    EnvironmentVariables = Settings.Environment,
                })
            );
        })
        .Finally(() => {
            if (!GetFiles("test/*/*.csproj").Any()) return;
            var coverageReport = CoverageFilePath("solution.dcvr");

            try {
                DotCoverMerge(GetCoverage("*.dcvr"), coverageReport);
            } catch {
                // Sometimes we come into this method so hot that dotcover is still working!
                System.Threading.Thread.Sleep(2);
                DotCoverMerge(GetCoverage("*.dcvr"), coverageReport);
            }

            DotCoverReport(
                coverageReport,
                CoverageFilePath("report/index.html"),
                new DotCoverReportSettings {
                    ReportType = DotCoverReportType.HTML
                });

            DotCoverReport(
                coverageReport,
                CoverageFilePath("solution.xml"),
                new DotCoverReportSettings {
                    ReportType = DotCoverReportType.DetailedXML
                });

            var covered = XmlPeek(CoverageFilePath("solution.xml"), "/Root/@CoveredStatements");
            var total =  XmlPeek(CoverageFilePath("solution.xml"), "/Root/@TotalStatements");
            var coverage = double.Parse(covered) / double.Parse(total);
            Information($"Code coverage: {coverage.ToString("P2")}");

            CleanBom(CoverageFilePath("solution.xml"));

            DotCoverToCoberturaSummary(
                coverageReport.ChangeExtension("xml"),
                coverageReport.ChangeExtension("cobertura"));
        });

Task("dotnet pack")
    .WithCriteria(() => Settings.Pack.Enabled)
    .IsDependeeOf("dotnet")
    .IsDependentOn("dotnet build")
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        MSBuild(solution, GoBuild("Pack")
            .WithProperty("NoBuild", (!Settings.Pack.Build).ToString())
            .WithProperty("RestoreNoCache", BuildSystem.IsLocalBuild.ToString())
            .WithProperty("RestoreForce", BuildSystem.IsLocalBuild.ToString())
            .WithProperty("IncludeSymbols", Settings.Pack.IncludeSymbols.ToString())
            .WithProperty("IncludeSource", Settings.Pack.IncludeSource.ToString())
            .WithProperty("PackageOutputPath", ArtifactDirectoryPath("nuget").MakeAbsolute(Context.Environment).FullPath)
        );
    });
