#addin "nuget:?package=Newtonsoft.Json"
#addin "nuget:?package=Rocket.Surgery.Cake&version={version}"

Task("Clean")
    .IsDependeeOf("Default")
    .Does(() => {
        CleanDirectory(Artifacts);
        EnsureDirectoryExists(Artifacts);
        CleanDirectory(Coverage);
        EnsureDirectoryExists(Coverage);
    });
