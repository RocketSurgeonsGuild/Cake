using Newtonsoft.Json.Serialization;

namespace Rocket.Surgery.Cake.Internal
{
    /// <summary>
    /// Class LowercaseNamingStrategy.
    /// Implements the <see cref="Newtonsoft.Json.Serialization.CamelCaseNamingStrategy" />
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Serialization.CamelCaseNamingStrategy" />
    class LowercaseNamingStrategy : CamelCaseNamingStrategy
    {
        protected override string ResolvePropertyName(string name) { return name.ToLowerInvariant(); }
    }
}
