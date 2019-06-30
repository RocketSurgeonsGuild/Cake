using System;
using Cake.Common.Build.TFBuild;
using Cake.Common.Build.TFBuild.Data;
using Cake.Core;
using Cake.Core.Annotations;

namespace Rocket.Surgery.Cake
{
    /// <summary>
    /// Class TFBuildCakeAliases.
    /// </summary>
    public static class TFBuildCakeAliases
    {
        /// <summary>
        /// Pulls the request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>TFBuildPullRequestInfo.</returns>
        [CakePropertyAlias(Cache = true)]
        [CakeNamespaceImport("Cake.Common.Build.TFBuild")]
        public static TFBuildPullRequestInfo PullRequest(this ICakeContext context)
        {
            return new TFBuildPullRequestInfo(context);
        }

        /// <summary>
        /// TFSs the source branch.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>System.String.</returns>
        [CakePropertyAlias(Cache = true)]
        public static string Tfs_SourceBranch(this ICakeContext context)
        {
            return context.Environment.GetEnvironmentVariable("BUILD_SOURCEBRANCH") ?? string.Empty;
        }

        /// <summary>
        /// TFSs the is tag.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        [CakePropertyAlias(Cache = true)]
        public static bool Tfs_IsTag(this ICakeContext context)
        {
            return Tfs_SourceBranch(context).IndexOf("/tags/", StringComparison.OrdinalIgnoreCase) > -1;
        }
    }

    /// <summary>
    /// Class TFBuildPullRequestInfo.
    /// </summary>
    public class TFBuildPullRequestInfo
    {
        private readonly ICakeContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TFBuildPullRequestInfo"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public TFBuildPullRequestInfo(ICakeContext context)
        {
            this._context = context;
        }
        /// <summary>
        /// Gets a value indicating whether this instance is pull request.
        /// </summary>
        /// <value><c>true</c> if this instance is pull request; otherwise, <c>false</c>.</value>
        public bool IsPullRequest => !string.IsNullOrEmpty(_context.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_PULLREQUESTID"));
        /// <summary>
        /// Gets the target branch.
        /// </summary>
        /// <value>The target branch.</value>
        public string TargetBranch => _context.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_TARGETBRANCH");
        /// <summary>
        /// Gets the source branch.
        /// </summary>
        /// <value>The source branch.</value>
        public string SourceBranch => _context.Environment.GetEnvironmentVariable("SYSTEM_PULLREQUEST_SOURCEBRANCH");
    }
}
