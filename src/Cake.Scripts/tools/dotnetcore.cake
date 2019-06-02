#load "./tools.cake"

DotNetCoreMSBuildSettings CreateDotNetCoreMsBuildSettings(string target)
{    return new DotNetCoreMSBuildSettings() {
        DiagnosticOutput = Settings.Diagnostic,
        DetailedSummary = Settings.Diagnostic,
        EnvironmentVariables = Settings.Environment,
        FileLoggers = {
            new MSBuildFileLoggerSettings {
                AppendToLogFile = false,
                LogFile = Artifact($"logs/{target.ToLower()}.log"),
                ShowTimestamp = true,
                Verbosity = Settings.DotNetCoreVerbosity,
                PerformanceSummary = Settings.Diagnostic,
                NoSummary = !Settings.Diagnostic,
                ShowCommandLine = Settings.Diagnostic,
            },
        },
    };
}

ProcessArgumentBuilder CreateBinLogger(ProcessArgumentBuilder a, string target) {
    return a.Append($"/bl:{Artifact($"logs/{target.ToLower()}.binlog")};ProjectImports={(!BuildSystem.IsLocalBuild || Settings.Diagnostic ? MSBuildBinaryLogImports.Embed : MSBuildBinaryLogImports.None)}");
}

Task("dotnetcore");

Task("dotnetcore restore")
    .IsDependeeOf("dotnetcore")
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        DotNetCoreRestore(
            solution.GetDirectory().FullPath,
            new DotNetCoreRestoreSettings() {
                Verbosity = Settings.DotNetCoreVerbosity,
                EnvironmentVariables = Settings.Environment,
                ArgumentCustomization = a => CreateBinLogger(a, "restore"),
                MSBuildSettings = CreateDotNetCoreMsBuildSettings("restore")
            });
    });

Task("dotnetcore build")
    .IsDependeeOf("dotnetcore")
    .IsDependentOn("dotnetcore restore")
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        DotNetCoreBuild(
            solution.FullPath,
            new DotNetCoreBuildSettings() {
                Verbosity = Settings.DotNetCoreVerbosity,
                Configuration = Settings.Configuration,
                EnvironmentVariables = Settings.Environment,
                ArgumentCustomization = a => CreateBinLogger(a, "build"),
                MSBuildSettings = CreateDotNetCoreMsBuildSettings("build")
            }
        );
    });

Task("dotnetcore test")
    .WithCriteria(() => Settings.XUnit.Enabled)
    .IsDependeeOf("dotnetcore")
    .IsDependentOn("dotnetcore build")
    .Does(() => {
        EnsureDirectoryExists(Artifact("test"));
        EnsureDirectoryExists(Coverage);
        EnsureDirectoryExists(CoverageDirectoryPath("report"));
    })
    .DoesForEach(
        GetFiles("*.sln"), (solution) => {
            var unitTestReport = ArtifactFilePath($"test/solution.xml")
                .MakeAbsolute(Context.Environment).FullPath;

            DotNetCoreTest(
                solution.FullPath,
                new DotNetCoreTestSettings() {
                    // Configuration = Settings.Configuration,
                    Configuration = "Debug",
                    DiagnosticOutput = Settings.Diagnostic,
                    Verbosity = Settings.DotNetCoreVerbosity,
                    // NoBuild = !Settings.XUnit.Build,
                    // NoRestore = !Settings.XUnit.Restore,
                    TestAdapterPath = ".",
                    Logger = $"\"xunit;LogFilePath={unitTestReport}\"",
                    ArgumentCustomization = args => CreateBinLogger(args, $"test")
                        .AppendSwitchQuoted("/p:CollectCoverage", "=", "true")
                        .AppendSwitchQuoted("/p:CoverageDirectory", "=", Coverage.FullPath)
                }
            );
        })
        .Finally(() => {
            if (!GetFiles("test/*/*.csproj").Any()) return;

            DotNetCoreExecute(
                Context.Tools.Resolve("ReportGenerator.dll"),
                $"-reports:{Coverage.FullPath}/**/*.cobertura.xml -targetdir:{Coverage.FullPath}/report -reporttypes:\"HTMLInline;HTMLSummary;TextSummary;Badges\"",
                new DotNetCoreExecuteSettings() {
                    WorkingDirectory = Context.Environment.WorkingDirectory
                });

            MergeCoberturaFiles(
                GetCoverage("**/*.cobertura.xml"),
                CoverageFilePath("solution.cobertura"));
        });

Task("dotnetcore pack")
    .WithCriteria(() => Settings.Pack.Enabled)
    .IsDependeeOf("dotnetcore")
    .IsDependentOn("dotnetcore build")
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        DotNetCorePack(
            solution.FullPath,
            new DotNetCorePackSettings() {
                Verbosity = Settings.DotNetCoreVerbosity,
                Configuration = Settings.Configuration,
                EnvironmentVariables = Settings.Environment,
                IncludeSource = Settings.Pack.IncludeSource,
                IncludeSymbols = Settings.Pack.IncludeSymbols,
                NoRestore = !BuildSystem.IsLocalBuild,
                NoBuild = !Settings.Pack.Build,
                OutputDirectory = ArtifactDirectoryPath("nuget").MakeAbsolute(Context.Environment).FullPath,
                ArgumentCustomization = a => CreateBinLogger(a, "pack"),
                MSBuildSettings = CreateDotNetCoreMsBuildSettings("pack")
                    .WithProperty("RestoreNoCache", BuildSystem.IsLocalBuild.ToString()),
            }
        );
    });
