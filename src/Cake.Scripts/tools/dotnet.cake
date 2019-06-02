#load "./tools.cake"

MSBuildSettings CreateMSBuildSettings(string target)
{
    return new MSBuildSettings {
        Targets = { target },
        EnvironmentVariables = Settings.Environment,
        Configuration = Settings.Configuration,
        DetailedSummary = Settings.Diagnostic,
        Verbosity = Settings.Verbosity,
        FileLoggers = {
            new MSBuildFileLogger {
                AppendToLogFile = false,
                LogFile = Artifact($"logs/{target.ToLower()}.log"),
                ShowTimestamp = true,
                Verbosity = Settings.Verbosity,
                PerformanceSummaryEnabled = Settings.Diagnostic,
                SummaryDisabled = Settings.Diagnostic,
                ShowCommandLine = Settings.Diagnostic,
            }
        },
        BinaryLogger = new MSBuildBinaryLogSettings {
            Enabled = true,
            FileName = Artifact($"logs/{target.ToLower()}.binlog"),
            Imports = BuildSystem.IsLocalBuild || Settings.Diagnostic ? MSBuildBinaryLogImports.Embed : MSBuildBinaryLogImports.None,
        }
    };
}

ProcessArgumentBuilder CreateBinLogger2(ProcessArgumentBuilder a, string target) {
    return a.Append($"/bl:{Artifact($"logs/{target.ToLower()}.binlog")};ProjectImports={(!BuildSystem.IsLocalBuild || Settings.Diagnostic ? MSBuildBinaryLogImports.Embed : MSBuildBinaryLogImports.None)}");
}

Task("dotnet");

Task("dotnet restore")
    .IsDependeeOf("dotnet")
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        MSBuild(solution, CreateMSBuildSettings("Restore").SetVerbosity(Settings.Verbosity));
    });

Task("dotnet build")
    .IsDependeeOf("dotnet")
    .IsDependentOn("dotnet restore")
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        MSBuild(solution, CreateMSBuildSettings("Build").SetVerbosity(Settings.Verbosity));
    });

Task("dotnet test")
    .WithCriteria(() => Settings.XUnit.Enabled)
    .IsDependeeOf("dotnet")
    .IsDependentOn("dotnet build")
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
                    Configuration = Settings.Configuration,
                    // Configuration = "Debug",
                    DiagnosticOutput = Settings.Diagnostic,
                    Verbosity = Settings.DotNetCoreVerbosity,
                    // NoBuild = !Settings.XUnit.Build,
                    // NoRestore = !Settings.XUnit.Restore,
                    TestAdapterPath = ".",
                    Logger = $"\"xunit;LogFilePath={unitTestReport}\"",
                    ArgumentCustomization = args => CreateBinLogger2(args, $"test")
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

Task("dotnet pack")
    .WithCriteria(() => Settings.Pack.Enabled)
    .IsDependeeOf("dotnet")
    .IsDependentOn("dotnet build")
    .DoesForEach(GetFiles("*.sln"), (solution) => {
        MSBuild(solution, CreateMSBuildSettings("Pack")
            .WithProperty("NoBuild", (!Settings.Pack.Build).ToString())
            .WithProperty("RestoreNoCache", BuildSystem.IsLocalBuild.ToString())
            .WithProperty("RestoreForce", BuildSystem.IsLocalBuild.ToString())
            .WithProperty("IncludeSymbols", Settings.Pack.IncludeSymbols.ToString())
            .WithProperty("IncludeSource", Settings.Pack.IncludeSource.ToString())
            .WithProperty("PackageOutputPath", ArtifactDirectoryPath("nuget").MakeAbsolute(Context.Environment).FullPath)
        );
    });
