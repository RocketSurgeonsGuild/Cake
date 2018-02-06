#addin "nuget:?package=Newtonsoft.Json"

Task("Clean")
    .IsDependeeOf("Default")
    .Does(() => {
        CleanDirectory(Artifacts);
        EnsureDirectoryExists(Artifacts);
        CleanDirectory(Coverage);
        EnsureDirectoryExists(Coverage);
    });
