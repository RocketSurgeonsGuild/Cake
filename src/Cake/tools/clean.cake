Task("Clean")
    .IsDependeeOf("Default")
    .Does(() => {
        CleanDirectory(Artifacts);
        EnsureDirectoryExists(Artifacts);
    });
