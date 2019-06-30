using Cake.Core;
using Cake.Core.Annotations;
using Newtonsoft.Json.Converters;
using Rocket.Surgery.Cake.TfsTasks;

namespace Rocket.Surgery.Cake
{
    /// <summary>
    /// Class TfsCakeAliases.
    /// </summary>
    public static class TfsCakeAliases
    {
        /// <summary>
        /// TFSs the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Tfs.</returns>
        [CakePropertyAlias(Cache = true)]
        [CakeNamespaceImport("Rocket.Surgery.Cake.TfsTasks")]
        public static Tfs Tfs(this ICakeContext context)
        {
            return new Tfs(context);
        }
    }
}
