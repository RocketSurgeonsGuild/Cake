using Newtonsoft.Json.Serialization;

namespace Rocket.Surgery.Cake.Internal
{
    class LowercaseNamesContractResolver : CamelCasePropertyNamesContractResolver
    {
        public LowercaseNamesContractResolver()
        {
            NamingStrategy = new LowercaseNamingStrategy { ProcessDictionaryKeys = true, OverrideSpecifiedNames = true };
        }
    }
}
