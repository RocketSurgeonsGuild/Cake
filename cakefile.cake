#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.1.1";

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

MSBuildSettings GoBuild(string target)
{
    return new MSBuildSettings {
        Targets = { target },
        EnvironmentVariables = Settings.Environment,
        Configuration = Configuration,
        DetailedSummary = true,
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
            Imports = MSBuildBinaryLogImports.Embed,
        }
    };
}

Task("dotnet2");

Task("dotnet2 restore")
    .IsDependeeOf("dotnet2")
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        MSBuild(solution, GoBuild("Restore").SetVerbosity(Verbosity.Minimal));
    });

Task("dotnet2 build")
    .IsDependeeOf("dotnet2")
    .IsDependentOn("dotnet2 restore")
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        MSBuild(solution, GoBuild("Build").SetVerbosity(Verbosity.Minimal));
    });


Task("dotnet2 test")
    .WithCriteria(IsRunningOnUnix)
    .WithCriteria(() => Settings.XUnit.Enabled)
    .IsDependentOn("dotnet2 build")
    .Does(() => {
        EnsureDirectoryExists(Artifact("test"));
    })
    .DoesForEach(GetFiles("test/*/*.csproj"), (testProject) =>
    {
        DotNetCoreTest(
            testProject.GetDirectory().FullPath,
            new DotNetCoreTestSettings() {
                NoBuild = true,
                Framework = "netcoreapp2.0",
                EnvironmentVariables = Settings.Environment,
        });
    });

Task("dotnet2 test w/coverage")
    .WithCriteria(IsRunningOnWindows)
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
                        .AppendSwitchQuoted("-xml", unitTestReport)
                        .AppendSwitchQuoted("-configuration", Configuration);

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
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        MSBuild(solution, GoBuild("Pack")
            .WithProperty("RestoreNoCache", BuildSystem.IsLocalBuild.ToString())
            .WithProperty("RestoreForce", BuildSystem.IsLocalBuild.ToString())
            .WithProperty("IncludeSymbols", Settings.Pack.IncludeSymbols.ToString())
            .WithProperty("IncludeSource", Settings.Pack.IncludeSource.ToString())
            .WithProperty("PackageOutputPath", ArtifactDirectoryPath("nuget").MakeAbsolute(Context.Environment).FullPath)
        );
    });


RunTarget(Target);
