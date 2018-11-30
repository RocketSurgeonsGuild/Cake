#addin "nuget:?package=Rocket.Surgery.Cake&version={version}"
#addin "nuget:?package=Newtonsoft.Json&version=11.0.2"

Task("Clean")
    .IsDependeeOf("Default")
    .Does(() => {
        CleanDirectory(Artifacts);
        EnsureDirectoryExists(Artifacts);
        CleanDirectory(Coverage);
        EnsureDirectoryExists(Coverage);
    });
