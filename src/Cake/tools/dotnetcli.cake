#tool "nuget:?package=JetBrains.dotCover.CommandLineTools"
#tool "nuget:?package=ReportUnit"

Task("dotnet");

Task("dotnet restore")
    .IsDependeeOf("dotnet")
    .Does(() => {
        DotNetCoreRestore(new DotNetCoreRestoreSettings () {
            NoCache = !BuildSystem.IsLocalBuild,
            EnvironmentVariables = Settings.Environment
        });
    });

Task("dotnet build")
    .IsDependeeOf("dotnet")
    .IsDependentOn("dotnet restore")
    .Does(() => {
        DotNetCoreMSBuild(new DotNetCoreMSBuildSettings() {
            EnvironmentVariables = Settings.Environment,
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

Task("dotnet test")
    .WithCriteria(() => Settings.XUnit.Enabled)
    .IsDependeeOf("dotnet")
    .IsDependentOn("dotnet build")
    .Does(() => {
        EnsureDirectoryExists(Artifact("test"));
        EnsureDirectoryExists(Artifact("coverage"));
        EnsureDirectoryExists(Artifact("report"));
    })
    .DoesForEach(
        GetFiles("test/*/*.csproj"),
        (file) => {
            var unitTestReport = new FilePath(Artifact($"test/{file.GetFilenameWithoutExtension().ToString()}.xml")).MakeAbsolute(Context.Environment).FullPath;

            var process = new ProcessArgumentBuilder();

            if (!Settings.XUnit.Shadow) process.Append("-noshadow");
            if (!Settings.XUnit.Build) process.Append("-nobuild");

            process.AppendSwitchQuoted("-xml", unitTestReport);

            DotCoverCover(tool => {
                tool.DotNetCoreTool(
                    file,
                    "xunit",
                    process,
                    new DotNetCoreToolSettings() {
                        EnvironmentVariables = Settings.Environment,
                        WorkingDirectory = file.GetDirectory()
                    });
                },
                new FilePath(Artifact($"coverage/{file.GetFilenameWithoutExtension().ToString()}.dcvr")).MakeAbsolute(Context.Environment),
                Settings.Coverage.Apply(new DotCoverCoverSettings() {
                    TargetWorkingDir = file.GetDirectory(),
                    EnvironmentVariables = Settings.Environment,
                })
            );
        })
        .Finally(() => {
            if (!GetFiles("test/*/*.csproj").Any()) return;
            var coverageReport = Artifact("coverage/solution.dcvr");

            try {
                DotCoverMerge(GetArtifacts("coverage/*.dcvr"), coverageReport);
            } catch {
                // Sometimes we come into this method so hot that dotcover is still working!
                System.Threading.Thread.Sleep(2);
                DotCoverMerge(GetArtifacts("coverage/*.dcvr"), coverageReport);
            }

            DotCoverReport(
                coverageReport,
                Artifact("report/coverage/index.html"),
                new DotCoverReportSettings {
                    ReportType = DotCoverReportType.HTML
                });

            DotCoverReport(
                coverageReport,
                Artifact("coverage/solution.xml"),
                new DotCoverReportSettings {
                    ReportType = DotCoverReportType.DetailedXML
                });

            ReportUnit(Artifact("test"), Artifact("report/xunit"));

            var covered = XmlPeek(Artifact("coverage/solution.xml"), "/Root/@CoveredStatements");
            var total =  XmlPeek(Artifact("coverage/solution.xml"), "/Root/@TotalStatements");
            var coverage = double.Parse(covered) / double.Parse(total);
            Information($"Code coverage: {coverage.ToString("P2")}");

            CleanBom(Artifact("coverage/solution.xml"));
        });

Task("dotnet pack")
    .WithCriteria(() => Settings.Pack.Enabled)
    .IsDependeeOf("dotnet")
    .IsDependentOn("dotnet build")
    .Does(() => {
        foreach (var project in GetFiles("src/*/*.csproj")
            .Where(z => !Settings.Pack.ExcludePaths.Any(x => !z.FullPath.Contains(x)))
        ) {
            DotNetCorePack(project.GetDirectory().FullPath, new DotNetCorePackSettings() {
                OutputDirectory = Artifact("nuget"),
                Configuration = Configuration,
                NoBuild = !Settings.Pack.Build,
                IncludeSymbols = Settings.Pack.IncludeSymbols,
                IncludeSource = Settings.Pack.IncludeSource,
                EnvironmentVariables = Settings.Environment
            });
        }
    });
