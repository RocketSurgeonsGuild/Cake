using System;
using System.Collections.Generic;
using Cake.Common.Tools.GitVersion;

namespace Rocket.Surgery.Cake
{
    public class Settings
    {
        public Settings(GitVersion version, Dictionary<string, string> environment)
        {
            Version = version;
            Environment = environment;
        }

        public XUnitSettings XUnit { get; } = new XUnitSettings();
        public PackSettings Pack { get; } = new PackSettings();
        public GitVersion Version { get; }
        public Dictionary<string, string> Environment { get; }


        public class XUnitSettings
        {
            public bool Build { get; set; }
            public bool Shadow { get; set; }
        }

        public class PackSettings
        {
            public bool Build { get; set; }
            public bool IncludeSymbols { get; set; } = true;
            public bool IncludeSource { get; set; } = true;
            public IEnumerable<string> ExcludePaths { get; set; } = Array.Empty<string>();
        }
    }
}
