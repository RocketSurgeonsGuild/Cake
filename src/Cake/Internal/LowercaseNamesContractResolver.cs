using Newtonsoft.Json.Serialization;

namespace Rocket.Surgery.Cake.Internal
{
    /// <summary>
    /// LowercaseNamesContractResolver.
    /// Implements the <see cref="Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver" />
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver" />
    class LowercaseNamesContractResolver : CamelCasePropertyNamesContractResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LowercaseNamesContractResolver"/> class.
        /// </summary>
        public LowercaseNamesContractResolver()
        {
            NamingStrategy = new LowercaseNamingStrategy { ProcessDictionaryKeys = true, OverrideSpecifiedNames = true };
        }
    }
}
