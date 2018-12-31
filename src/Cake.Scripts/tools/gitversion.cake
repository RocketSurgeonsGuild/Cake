#load "./tools.cake"

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
