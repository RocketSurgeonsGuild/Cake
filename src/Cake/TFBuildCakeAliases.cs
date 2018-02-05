using System;
using Cake.Common.Build.TFBuild;
using Cake.Common.Build.TFBuild.Data;
using Cake.Core;
using Cake.Core.Annotations;

namespace Rocket.Surgery.Cake
{
    public static class TFBuildCakeAliases
    {
        [CakePropertyAlias(Cache = true)]
        [CakeNamespaceImport("Cake.Common.Build.TFBuild")]
        public static TFBuildPullRequestInfo PullRequest(this ICakeContext context)
        {
            return new TFBuildPullRequestInfo(context);
        }

        [CakePropertyAlias(Cache = true)]
        public static string Tfs_SourceBranch(this ICakeContext context)
        {
            return context.Environment.GetEnvironmentVariable("BUILD_SOURCEBRANCH") ?? string.Empty;
        }

        [CakePropertyAlias(Cache = true)]
        public static bool Tfs_IsTag(this ICakeContext context)
        {
            return Tfs_SourceBranch(context).IndexOf("/tags/", StringComparison.OrdinalIgnoreCase) > -1;
        }
    }

    public class TFBuildPullRequestInfo
    {
        private readonly ICakeContext _context;

        public TFBuildPullRequestInfo(ICakeContext context)
        {
            this._context = context;
        }
        public bool IsPullRequest => !string.IsNullOrEmpty(_context.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_PULLREQUESTID"));
        public string TargetBranch => _context.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_TARGETBRANCH");
        public string SourceBranch => _context.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_SOURCEBRANCH");
    }
}
