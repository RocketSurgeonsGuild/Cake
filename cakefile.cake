#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.1.4";

Task("Default")
    .IsDependentOn("PinVersion")
    .IsDependentOn("dotnet")
    .IsDependentOn("dotnet2 test")
    .IsDependentOn("dotnet3 test");

Task("dotnet2 test")
    .WithCriteria(IsRunningOnUnix)
    .WithCriteria(() => Settings.XUnit.Enabled)
    .IsDependeeOf("dotnet")
    .IsDependentOn("dotnet build")
    .Does(() => {
        EnsureDirectoryExists(Artifact("test"));
    })
    .DoesForEach(GetFiles("test/*/*.csproj"), (testProject) =>
    {
        DotNetCoreTest(
            testProject.GetDirectory().FullPath,
            new DotNetCoreTestSettings() {
                NoBuild = true,
                Configuration = Configuration,
                Framework = "netcoreapp2.0",
                EnvironmentVariables = Settings.Environment,
        });
    });

Task("dotnet3 test")
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
                .MakeAbsolute(Context.Environment).FullPath.Replace("/", "\\");

            var process = new ProcessArgumentBuilder()
                        .AppendSwitchQuoted("-xml", unitTestReport)
                        .AppendSwitchQuoted("-configuration", Configuration);

            if (!Settings.XUnit.Shadow) process.Append("-noshadow");
            if (!Settings.XUnit.Build) process.Append("-nobuild");

            DotNetCoreTool(
                file,
                "xunit",
                process,
                new DotNetCoreToolSettings() {
                    EnvironmentVariables = Settings.Environment,
                }
            );
        })
        .Finally(() => {
            if (!GetFiles("test/*/*.csproj").Any()) return;

            ReportUnit(Artifact("test"), Artifact("report/xunit"));
        });

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

RunTarget(Target);
