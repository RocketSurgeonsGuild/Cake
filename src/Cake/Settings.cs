using System;
using System.Collections.Generic;
using Cake.Common.Tools.DotCover;
using Cake.Common.Tools.DotCover.Cover;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.GitVersion;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Common.Tools.MSBuild;

namespace Rocket.Surgery.Cake
{
    public class Settings
    {
        public Settings(
            GitVersion version,
            Dictionary<string, string> environment,
            string configuration,
            Verbosity verbosity
        )
        {
            Version = version;
            Environment = environment;
            Configuration = configuration;
            Verbosity = verbosity;
            Diagnostic = verbosity > Verbosity.Normal;
        }

        public XUnitSettings XUnit { get; } = new XUnitSettings();
        public PackSettings Pack { get; } = new PackSettings();
        public CoverageSettings Coverage { get; } = new CoverageSettings();
        public GitVersion Version { get; }
        public Dictionary<string, string> Environment { get; }
        public string Configuration { get; }
        public Verbosity Verbosity { get; set; }

        public DotNetCoreVerbosity DotNetCoreVerbosity
        {
            get
            {
                switch (Verbosity)
                {
                    case Verbosity.Quiet: return DotNetCoreVerbosity.Quiet;
                    case Verbosity.Minimal: return DotNetCoreVerbosity.Minimal;
                    case Verbosity.Normal: return DotNetCoreVerbosity.Normal;
                    case Verbosity.Verbose: return DotNetCoreVerbosity.Detailed;
                    case Verbosity.Diagnostic: return DotNetCoreVerbosity.Diagnostic;
                }
                return DotNetCoreVerbosity.Normal;
            }
        }
        public bool Diagnostic { get; set; }

        public class XUnitSettings
        {
            public bool Enabled { get; set; } = true;
            public bool Build { get; set; } = false;
            public bool Restore { get; set; } = false;
            public bool Shadow { get; set; }
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
            public bool Build { get; set; } = false;
            public bool IncludeSymbols { get; set; } = true;
            public bool IncludeSource { get; set; } = true;
            public List<string> ExcludePaths { get; set; } = new List<string>();
        }
    }
}
