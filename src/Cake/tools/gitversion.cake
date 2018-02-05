#addin "nuget:?package=Newtonsoft.Json"
#addin "nuget:?package=Rocket.Surgery.Cake&version={version}"
#tool "nuget:?package=GitVersion.CommandLine&prerelease"

Task("GitVersion")
    .IsDependeeOf("Default")
    .WithCriteria(!BuildSystem.IsLocalBuild)
    .WithCriteria(FileExists("GitVersion.yml"))
    .Does(() =>{
    var gv = GitVersion(new GitVersionSettings() {
        UpdateAssemblyInfo = true,
        OutputType = GitVersionOutput.BuildServer,
    });
});
