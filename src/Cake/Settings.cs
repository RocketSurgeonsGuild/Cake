using System;
using System.Collections.Generic;
using Cake.Common.Tools.DotCover;
using Cake.Common.Tools.DotCover.Cover;
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
        public CoverageSettings Coverage { get; } = new CoverageSettings();
        public GitVersion Version { get; }
        public Dictionary<string, string> Environment { get; }


        public class XUnitSettings
        {
            public bool Enabled { get; set; } = true;
            public bool Build { get; set; }
            public bool Shadow { get; set; }
            public bool Verbose { get; set; };
        }

        public class CoverageSettings
        {
            public ISet<string> AttributeFilters { get; set; } = new HashSet<string>
            {
                "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
                "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute"
            };

            public ISet<string> Filters { get; set; } = new HashSet<string>
            {
                "+:Rocket.*", "-:*.Tests"
            };

            public DotCoverCoverSettings Apply(DotCoverCoverSettings settings)
            {
                foreach (var filter in AttributeFilters)
                {
                    settings.WithAttributeFilter(filter);
                }
                foreach (var filter in Filters)
                {
                    settings.WithFilter(filter);
                }
                return settings;
            }
        }

        public class PackSettings
        {
            public bool Enabled { get; set; } = true;
            public bool Build { get; set; }
            public bool IncludeSymbols { get; set; } = true;
            public bool IncludeSource { get; set; } = true;
            public List<string> ExcludePaths { get; set; } = new List<string>();
        }
    }
}
