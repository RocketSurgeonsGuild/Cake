using Newtonsoft.Json.Serialization;

namespace Rocket.Surgery.Cake.Internal
{
    class LowercaseNamingStrategy : CamelCaseNamingStrategy
    {
        protected override string ResolvePropertyName(string name) { return name.ToLowerInvariant(); }
    }
}
