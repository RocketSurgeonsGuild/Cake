#tool "nuget:?package=JetBrains.dotCover.CommandLineTools"
#tool "nuget:?package=ReportUnit"

Task("DotnetCore")
    .IsDependentOn("DotnetCoreRestore")
    .IsDependentOn("DotnetCoreBuild")
    .IsDependentOn("DotnetCoreTest")
    .IsDependentOn("DotnetCorePack");

Task("DotnetCoreRestore")
    .Does(() => {
        DotNetCoreRestore(new DotNetCoreRestoreSettings () {
            NoCache = !BuildSystem.IsLocalBuild,
            EnvironmentVariables = GitVersionEnvironmentVariables()
        });
    });

Task("DotnetCoreBuild")
    .IsDependentOn("DotnetCoreRestore")
    .Does(() => {
        DotNetCoreMSBuild(new DotNetCoreMSBuildSettings() {
            EnvironmentVariables = GitVersionEnvironmentVariables(),
            DetailedSummary = true,
        }
        .SetConfiguration(Configuration)
        .WithTarget("Build")
        .AddFileLogger(new MSBuildFileLoggerSettings() {
            AppendToLogFile = false,
            ForceNoAlign = true,
            LogFile = Artifact("logs/msbuild.log"),
            ShowTimestamp = true,
            Verbosity = DotNetCoreVerbosity.Detailed
        }));
    });

Task("DotnetCoreTest")
    .IsDependentOn("DotnetCoreBuild")
    .DoesForEach(GetFiles("test/*/*.csproj"), (file) => {
        var unitTestReport = new FilePath(Artifact($"test/{file.GetFilename().ToString()}.xml")).MakeAbsolute(Context.Environment).FullPath;

        DotCoverCover(tool => {
            tool.DotNetCoreTool(
                file.GetDirectory().FullPath,
                "xunit",
                new ProcessArgumentBuilder()
                    .AppendSwitchQuoted("-xml", unitTestReport)
                    .Append("-noshadow"),
                new DotNetCoreToolSettings() {
                    EnvironmentVariables = GitVersionEnvironmentVariables(),
                });
            },
            new FilePath(Artifact($"test/{file.GetFilename().ToString()}.dcvr")).MakeAbsolute(Context.Environment),
            new DotCoverCoverSettings() {
                TargetWorkingDir = file.GetDirectory(),
                EnvironmentVariables = GitVersionEnvironmentVariables(),
            }
            .WithAttributeFilter("System.Runtime.CompilerServices.CompilerGeneratedAttribute")
            .WithAttributeFilter("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute")
            .WithFilter("+:Rocket.*")
            .WithFilter("-:*.Tests")
        );
    })
    .Finally(() => {
        if (!GetFiles("test/*/*.csproj").Any()) return;
        var coverageReport = Artifact("test/solution.dcvr");

        try {
            DotCoverMerge(GetArtifacts("test/*.dcvr"), coverageReport);
        } catch {
            // Sometimes we come into this method so hot that dotcover is still working!
            System.Threading.Thread.Sleep(2);
            DotCoverMerge(GetArtifacts("test/*.dcvr"), coverageReport);
        }

        DotCoverReport(
            coverageReport,
            Artifact("report/coverage/index.html"),
            new DotCoverReportSettings {
                ReportType = DotCoverReportType.HTML
            });

        DotCoverReport(
            coverageReport,
            Artifact("report/coverage/index.xml"),
            new DotCoverReportSettings {
                ReportType = DotCoverReportType.DetailedXML
            });

        ReportUnit(Artifact("test"), Artifact("report/xunit"));

        var covered = XmlPeek(Artifact("report/coverage/index.xml"), "/Root/@CoveredStatements");
        var total =  XmlPeek(Artifact("report/coverage/index.xml"), "/Root/@TotalStatements");
        var coverage = double.Parse(covered) / double.Parse(total);
        Information($"Code coverage: {coverage.ToString("P2")}");

        CleanBom(Artifact("report/coverage/index.xml"));
    });

Task("DotnetCorePack")
    .IsDependentOn("DotnetCoreBuild")
    .Does(() => {
        foreach (var project in GetFiles("src/*/*.csproj").Where(z => !z.FullPath.Contains(".Functions"))) {
            DotNetCorePack(project.GetDirectory().FullPath, new DotNetCorePackSettings() {
                OutputDirectory = Artifact("nuget"),
                Configuration = Configuration,
                NoBuild = true,
                IncludeSymbols = true,
                IncludeSource = true,
                EnvironmentVariables = GitVersionEnvironmentVariables()
            });
        }
    });
