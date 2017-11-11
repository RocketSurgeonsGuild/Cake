using System.Collections.Generic;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;

namespace Rocket.Surgery.Cake
{
    public static class LocalCakeAliases
    {
        [CakePropertyAlias(Cache = true)]
        public static string Target(this ICakeContext context)
        {
            return context.Argument("target", "Build");
        }

        [CakePropertyAlias(Cache = true)]
        public static Settings Settings(this ICakeContext context)
        {
            var gv = GitVer(context);

            return new Settings(gv, GitVersionEnvironment(context, gv));
        }

        private static Dictionary<string, string> GitVersionEnvironment(this ICakeContext context, GitVersion version)
        {
            return new Dictionary<string, string>() {
                { "GitVersion_Major", version.Major.ToString() },
                { "GitVersion_Minor", version.Minor.ToString() },
                { "GitVersion_Patch", version.Patch.ToString() },
                { "GitVersion_PreReleaseTag", version.PreReleaseTag },
                { "GitVersion_PreReleaseTagWithDash", version.PreReleaseTagWithDash },
                { "GitVersion_PreReleaseLabel", version.PreReleaseLabel },
                { "GitVersion_PreReleaseNumber", version.PreReleaseNumber.ToString() },
                { "GitVersion_BuildMetaData", version.BuildMetaData },
                { "GitVersion_BuildMetaDataPadded", version.BuildMetaDataPadded },
                { "GitVersion_FullBuildMetaData", version.FullBuildMetaData },
                { "GitVersion_MajorMinorPatch", version.MajorMinorPatch },
                { "GitVersion_SemVer", version.SemVer },
                { "GitVersion_LegacySemVer", version.LegacySemVer },
                { "GitVersion_LegacySemVerPadded", version.LegacySemVerPadded },
                { "GitVersion_AssemblySemVer", version.AssemblySemVer },
                { "GitVersion_FullSemVer", version.FullSemVer },
                { "GitVersion_InformationalVersion", version.InformationalVersion },
                { "GitVersion_BranchName", version.BranchName },
                { "GitVersion_Sha", version.Sha },
                { "GitVersion_NuGetVersion", version.NuGetVersion },
                { "GitVersion_CommitsSinceVersionSource", version.CommitsSinceVersionSource.ToString() },
                { "GitVersion_CommitsSinceVersionSourcePadded", version.CommitsSinceVersionSourcePadded },
                { "GitVersion_CommitDate", version.CommitDate },
                { "PackageVersion" , version.NuGetVersion },
                { "AssemblyVersion", version.Major + "0.0.0" },
                { "FileVersion", version.AssemblySemVer },
                { "InformationalVersion", version.InformationalVersion },
            };
        }

        [CakePropertyAlias(Cache = true)]
        public static string Configuration(this ICakeContext context)
        {
            return context.Argument("configuration", "Debug");
        }

        [CakeMethodAlias]
        public static string Artifact(this ICakeContext context, string path)
        {
            return Artifacts(context) + "/" + path.TrimStart('/', '\\');
        }

        [CakePropertyAlias(Cache = true)]
        public static DirectoryPath Artifacts(this ICakeContext context)
        {
            return DirectoryPath.FromString("./artifacts");
        }

        [CakeMethodAlias]
        public static FilePath ArtifactFilePath(this ICakeContext context, string path)
        {
            return FilePath.FromString(Artifacts(context) + "/" + path.TrimStart('/', '\\'));
        }

        [CakeMethodAlias]
        public static DirectoryPath ArtifactDirectoryPath(this ICakeContext context, string path)
        {
            return DirectoryPath.FromString(Artifacts(context) + "/" + path.TrimStart('/', '\\'));
        }

        [CakePropertyAlias(Cache = true)]
        public static GitVersion GitVer(this ICakeContext context)
        {
            return context.GitVersion();
        }

        [CakeMethodAlias]
        public static IEnumerable<FilePath> GetArtifacts(this ICakeContext context, string glob)
        {
            return context.GetFiles($"{context.Artifacts()}/{glob}");
        }
    }
}
