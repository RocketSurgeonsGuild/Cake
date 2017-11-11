#addin "nuget:?package=Rocket.Surgery.Build.Cake&version=1.0.3"
#load "nuget:?package=Rocket.Surgery.Build.Cake&version=1.0.3";

Task("Default")
    .IsDependentOn("GitVersion")
    .IsDependentOn("CleanArtifacts")
    .IsDependentOn("PinVersion")
    .IsDependentOn("DotnetCore");

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

RunTarget(target);
