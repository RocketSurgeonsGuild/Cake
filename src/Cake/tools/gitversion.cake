#tool "nuget:?package=GitVersion.CommandLine&prerelease"

Task("GitVersion")
    .WithCriteria(!BuildSystem.IsLocalBuild)
    .WithCriteria(FileExists("GitVersion.yml"))
    .Does(() =>{
    var gv = GitVersion(new GitVersionSettings() {
        UpdateAssemblyInfo = true,
        OutputType = GitVersionOutput.BuildServer,
    });
});
