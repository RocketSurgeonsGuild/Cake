using Cake.Common.Tools.GitVersion;
using Cake.Core.IO;
using Semver;

namespace Rocket.Surgery.Cake
{
    public class Local
    {
        public Local(string target, string configuration, DirectoryPath artifacts, GitVersion gitVersion)
        {
            Target = target;
            Configuration = configuration;
            Artifacts = artifacts;
            GitVersion = gitVersion;
            NpmTag = GetNpmTag(GitVersion);
        }


        private string GetNpmTag(GitVersion gitVersion)
        {
            if (gitVersion.PreReleaseNumber == 0)
            {
                return "latest";
            }

            if (gitVersion.BranchName == "master" || gitVersion.BranchName.EndsWith("/master"))
            {
                return "next";
            }

            // sometimes we get the tag as the name
            if (SemVersion.TryParse(gitVersion.BranchName.TrimStart('v'), out var _))
            {
                return "next";
            }

            return gitVersion.BranchName;
        }

        public string Target { get; }
        public string Configuration { get; }
        public DirectoryPath Artifacts { get; }
        public GitVersion GitVersion { get; }
        public string NpmTag { get; }
    }
}
