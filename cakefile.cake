#load "nuget:?package=Rocket.Surgery.Cake.Library&version=0.4.2";

Task("Default")
    .IsDependentOn("PinVersion")
    .IsDependentOn("dotnet")
    //.IsDependentOn("TestScripts")
    ;

Task("PinVersion")
    .WithCriteria(!BuildSystem.IsLocalBuild)
    .Does(() => {
        foreach (var angel in GetFiles("./src/**/*.cake")) {
            PinVersion(angel, GitVer.NuGetVersion);
        }
    });

void PinVersion(FilePath file, string version) {
    var content = System.IO.File.ReadAllText(file.FullPath);
    if (content.IndexOf("{version}") > -1) {
        System.IO.File.WriteAllText(file.FullPath, content.Replace("{version}", version));
    }
}

Task("TestScripts")
    .IsDependentOn("dotnet")
    .DoesForEach(GetFiles("src/**/*.cake"), (sourceFile) => {
        var testFolder = Artifacts.Combine("testfolder");
        CleanDirectory(testFolder);
        EnsureDirectoryExists(testFolder);

        var nugetConfig = testFolder.CombineWithFilePath("NuGet.config");
        CopyFile("./NuGet.config", nugetConfig);
        NuGetAddSource(
            "testlocation",
            ArtifactDirectoryPath("nuget").MakeAbsolute(Context.Environment).FullPath.Replace("/", "\\"),
            new NuGetSourcesSettings() {
                ConfigFile = nugetConfig,
            });

        var testFile = testFolder.CombineWithFilePath(sourceFile.GetFilename());
        CopyFile(sourceFile, testFile);
        PinVersion(testFile, GitVer.NuGetVersion);

        try {
            CakeExecuteScript(testFolder.CombineWithFilePath(sourceFile.GetFilename()), new CakeSettings() {
                WorkingDirectory = testFolder
            });
        } catch {
            foreach (var angel in GetFiles(testFolder.FullPath + "/**/*.cake")) {
                PinVersion(angel, GitVer.NuGetVersion);
            }
            CakeExecuteScript(testFolder.CombineWithFilePath(sourceFile.GetFilename()), new CakeSettings() {
                WorkingDirectory = testFolder
            });
        }
    })
    .Does(() => {
        var testFolder = Artifacts.Combine("testfolder");
        CleanDirectory(testFolder);
    });

RunTarget(Target);
