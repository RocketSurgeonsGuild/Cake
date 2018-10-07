#addin "nuget:?package=Rocket.Surgery.Cake&version={version}"
#tool "nuget:?package=GitVersion.CommandLine&prerelease&version=4.0.0-beta0012"

Task("GitVersion")
    .IsDependeeOf("Default")
    .WithCriteria(!BuildSystem.IsLocalBuild)
    .WithCriteria(!HasGitVer)
    .WithCriteria(FileExists("GitVersion.yml"))
    .Does(() =>{
    var gv = GitVersion(new GitVersionSettings() {
        UpdateAssemblyInfo = true,
        OutputType = GitVersionOutput.BuildServer,
    });
});
