using System.Collections.Generic;
using Cake.Core;
using Cake.Core.Annotations;

namespace Rocket.Surgery.Cake
{
    public static class GitVersionAliases
    {
        private static Dictionary<string, string> __GitVersionEnvironmentVariables__;

        [CakeMethodAlias]
        public static Dictionary<string, string> GitVersionEnvironmentVariables(this ICakeContext context)
        {
            if (__GitVersionEnvironmentVariables__ == null)
            {
                var gv = context.Local().GitVersion;

                __GitVersionEnvironmentVariables__ = new Dictionary<string, string>() {
                    { "GitVersion_Major", gv.Major.ToString() },
                    { "GitVersion_Minor", gv.Minor.ToString() },
                    { "GitVersion_Patch", gv.Patch.ToString() },
                    { "GitVersion_PreReleaseTag", gv.PreReleaseTag },
                    { "GitVersion_PreReleaseTagWithDash", gv.PreReleaseTagWithDash },
                    { "GitVersion_PreReleaseLabel", gv.PreReleaseLabel },
                    { "GitVersion_PreReleaseNumber", gv.PreReleaseNumber.ToString() },
                    { "GitVersion_BuildMetaData", gv.BuildMetaData },
                    { "GitVersion_BuildMetaDataPadded", gv.BuildMetaDataPadded },
                    { "GitVersion_FullBuildMetaData", gv.FullBuildMetaData },
                    { "GitVersion_MajorMinorPatch", gv.MajorMinorPatch },
                    { "GitVersion_SemVer", gv.SemVer },
                    { "GitVersion_LegacySemVer", gv.LegacySemVer },
                    { "GitVersion_LegacySemVerPadded", gv.LegacySemVerPadded },
                    { "GitVersion_AssemblySemVer", gv.AssemblySemVer },
                    { "GitVersion_FullSemVer", gv.FullSemVer },
                    { "GitVersion_InformationalVersion", gv.InformationalVersion },
                    { "GitVersion_BranchName", gv.BranchName },
                    { "GitVersion_Sha", gv.Sha },
                    { "GitVersion_NuGetVersion", gv.NuGetVersion },
                    { "GitVersion_CommitsSinceVersionSource", gv.CommitsSinceVersionSource.ToString() },
                    { "GitVersion_CommitsSinceVersionSourcePadded", gv.CommitsSinceVersionSourcePadded },
                    { "GitVersion_CommitDate", gv.CommitDate },
                    { "PackageVersion" , gv.NuGetVersion },
                    { "AssemblyVersion", gv.Major + "0.0.0" },
                    { "FileVersion", gv.AssemblySemVer },
                    { "InformationalVersion", gv.InformationalVersion },
                };
            }
            return __GitVersionEnvironmentVariables__;
        }
    }
}
