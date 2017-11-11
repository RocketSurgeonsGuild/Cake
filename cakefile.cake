#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.1.1-beta0009";

Task("Default")
    .IsDependentOn("PinVersion")
    .IsDependentOn("dotnet2");

Task("PinVersion")
    .WithCriteria(!BuildSystem.IsLocalBuild)
    .Does(() => {
        foreach (var angel in GetFiles("./src/**/angel.cake")) {
            var content = System.IO.File.ReadAllText(angel.FullPath);
            if (content.IndexOf("{version}") > -1) {
                System.IO.File.WriteAllText(angel.FullPath, content.Replace("{version}", GitVer.NuGetVersion));
            }
        }
    });

Task("dotnet2");

Task("dotnet2 restore")
    .IsDependeeOf("dotnet2")
    .Does(() => {
        DotNetCoreRestore(new DotNetCoreRestoreSettings () {
            NoCache = !BuildSystem.IsLocalBuild,
            EnvironmentVariables = Settings.Environment
        });
    });

Task("dotnet2 build")
    .IsDependeeOf("dotnet2")
    .IsDependentOn("dotnet2 restore")
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

Task("dotnet2 test")
    .WithCriteria(() => Settings.XUnit.Enabled)
    .IsDependeeOf("dotnet2")
    .IsDependentOn("dotnet2 build")
    .Does(() => {
        EnsureDirectoryExists(Artifact("test"));
        EnsureDirectoryExists(Artifact("coverage"));
        EnsureDirectoryExists(Artifact("report"));
    })
    .DoesForEach(
        GetFiles("test/*/*.csproj"),
        (file) => {
            var unitTestReport = ArtifactFilePath($"test/{file.GetFilenameWithoutExtension().ToString()}.xml")
                .MakeAbsolute(Context.Environment).FullPath.Replace("/", "\\");

            var process = new ProcessArgumentBuilder()
                        .AppendSwitchQuoted("-xml", unitTestReport);

            if (!Settings.XUnit.Shadow) process.Append("-noshadow");
            if (!Settings.XUnit.Build) process.Append("-nobuild");

            DotCoverCover(tool => {
            tool.DotNetCoreTool(
                file,
                "xunit",
                process,
                new DotNetCoreToolSettings() {
                    EnvironmentVariables = Settings.Environment,
                });
            },
            new FilePath(Artifact($"coverage/{file.GetFilenameWithoutExtension().ToString()}.dcvr")).MakeAbsolute(Context.Environment),
            new DotCoverCoverSettings() {
                WorkingDirectory = file.GetDirectory(),
                TargetWorkingDir = file.GetDirectory(),
                EnvironmentVariables = Settings.Environment,
            });
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
                ArtifactFilePath("report/coverage/index.html"),
                new DotCoverReportSettings {
                    ReportType = DotCoverReportType.HTML
                });

            DotCoverReport(
                coverageReport,
                ArtifactFilePath("coverage/solution.xml"),
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

Task("dotnet2 pack")
    .WithCriteria(() => Settings.Pack.Enabled)
    .IsDependeeOf("dotnet2")
    .IsDependentOn("dotnet2 build")
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


RunTarget(Target);
