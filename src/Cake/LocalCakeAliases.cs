using System;
using System.Collections.Generic;
using System.Linq;
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
            return DirectoryPath.FromString(context.Argument("artifacts", "./artifacts"));
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
        public static DirectoryPath Coverage(this ICakeContext context)
        {
            return DirectoryPath.FromString(context.Argument("coverage", "./coverage"));
        }

        [CakeMethodAlias]
        public static FilePath CoverageFilePath(this ICakeContext context, string path)
        {
            return FilePath.FromString(Coverage(context) + "/" + path.TrimStart('/', '\\'));
        }

        [CakeMethodAlias]
        public static DirectoryPath CoverageDirectoryPath(this ICakeContext context, string path)
        {
            return DirectoryPath.FromString(Coverage(context) + "/" + path.TrimStart('/', '\\'));
        }

        private static readonly string[] GitVersionKeys = {
            "GITVERSION_MAJOR",
            "GITVERSION_MINOR",
            "GITVERSION_PATCH",
            "GITVERSION_PRERELEASETAG",
            "GITVERSION_PRERELEASETAGWITHDASH",
            "GITVERSION_PRERELEASELABEL",
            "GITVERSION_PRERELEASENUMBER",
            "GITVERSION_BUILDMETADATA",
            "GITVERSION_BUILDMETADATAPADDED",
            "GITVERSION_FULLBUILDMETADATA",
            "GITVERSION_MAJORMINORPATCH",
            "GITVERSION_SEMVER",
            "GITVERSION_LEGACYSEMVER",
            "GITVERSION_LEGACYSEMVERPADDED",
            "GITVERSION_ASSEMBLYSEMVER",
            "GITVERSION_FULLSEMVER",
            "GITVERSION_INFORMATIONALVERSION",
            "GITVERSION_BRANCHNAME",
            "GITVERSION_SHA",
            "GITVERSION_NUGETVERSION",
            "GITVERSION_COMMITSSINCEVERSIONSOURCE",
            "GITVERSION_COMMITSSINCEVERSIONSOURCEPADDED",
            "GITVERSION_COMMITDATE",
        };

        [CakePropertyAlias(Cache = true)]
        public static bool HasGitVer(this ICakeContext context)
        {
            return GitVersionKeys.Any(z => !string.IsNullOrWhiteSpace( context.EnvironmentVariable(z)));
        }

        internal static GitVersion _gitVersion;

        [CakePropertyAlias(Cache = true)]
        public static GitVersion GitVer(this ICakeContext context)
        {
            var environmentVariables = context.Environment.GetEnvironmentVariables();
            if (HasGitVer(context))
            {
                return _gitVersion ?? ( _gitVersion = new GitVersion()
                {
                    Major = int.Parse(environmentVariables["GitVersion_Major"]),
                    Minor = int.Parse(environmentVariables["GitVersion_Minor"]),
                    Patch = int.Parse(environmentVariables["GitVersion_Patch"]),
                    PreReleaseTag = environmentVariables["GitVersion_PreReleaseTag"],
                    PreReleaseTagWithDash = environmentVariables["GitVersion_PreReleaseTagWithDash"],
                    PreReleaseLabel = environmentVariables["GitVersion_PreReleaseLabel"],
                    PreReleaseNumber = int.Parse(environmentVariables["GitVersion_PreReleaseNumber"]),
                    BuildMetaData = environmentVariables["GitVersion_BuildMetaData"],
                    BuildMetaDataPadded = environmentVariables["GitVersion_BuildMetaDataPadded"],
                    FullBuildMetaData = environmentVariables["GitVersion_FullBuildMetaData"],
                    MajorMinorPatch = environmentVariables["GitVersion_MajorMinorPatch"],
                    SemVer = environmentVariables["GitVersion_SemVer"],
                    LegacySemVer = environmentVariables["GitVersion_LegacySemVer"],
                    LegacySemVerPadded = environmentVariables["GitVersion_LegacySemVerPadded"],
                    AssemblySemVer = environmentVariables["GitVersion_AssemblySemVer"],
                    FullSemVer = environmentVariables["GitVersion_FullSemVer"],
                    InformationalVersion = environmentVariables["GitVersion_InformationalVersion"],
                    BranchName = environmentVariables["GitVersion_BranchName"],
                    Sha = environmentVariables["GitVersion_Sha"],
                    NuGetVersion = environmentVariables["GitVersion_NuGetVersion"],
                    CommitsSinceVersionSource = string.IsNullOrWhiteSpace(environmentVariables["GitVersion_CommitsSinceVersionSource"]) ? null : int.Parse(environmentVariables["GitVersion_CommitsSinceVersionSource"]) as int?,
                    CommitsSinceVersionSourcePadded = environmentVariables["GitVersion_CommitsSinceVersionSourcePadded"],
                    CommitDate = environmentVariables["GitVersion_CommitDate"],
                });
            }
            else
            {
                return _gitVersion ?? (_gitVersion = context.GitVersion());
            }
        }

        [CakeMethodAlias]
        public static IEnumerable<FilePath> GetArtifacts(this ICakeContext context, string glob)
        {
            return context.GetFiles($"{context.Artifacts()}/{glob}");
        }

        [CakeMethodAlias]
        public static IEnumerable<FilePath> GetCoverage(this ICakeContext context, string glob)
        {
            return context.GetFiles($"{context.Coverage()}/{glob}");
        }
    }
}
