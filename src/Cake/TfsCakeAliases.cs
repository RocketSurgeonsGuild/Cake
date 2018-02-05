using Cake.Core;
using Cake.Core.Annotations;
using Newtonsoft.Json.Converters;
using Rocket.Surgery.Cake.TfsTasks;

namespace Rocket.Surgery.Cake
{
    public static class TfsCakeAliases
    {
        [CakePropertyAlias(Cache = true)]
        [CakeNamespaceImport("Rocket.Surgery.Cake.TfsTasks")]
        public static Tfs Tfs(this ICakeContext context)
        {
            return new Tfs(context);
        }
    }
}
