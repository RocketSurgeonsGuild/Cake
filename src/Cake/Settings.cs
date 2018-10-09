using System;
using System.Collections.Generic;
using Cake.Common.Tools.DotCover;
using Cake.Common.Tools.DotCover.Cover;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.GitVersion;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

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
        }

        public XUnitSettings XUnit { get; } = new XUnitSettings();
        public PackSettings Pack { get; } = new PackSettings();
        public CoverageSettings Coverage { get; } = new CoverageSettings();
        public GitVersion Version { get; }
        public Dictionary<string, string> Environment { get; }
        public string Configuration { get; }
        public Verbosity Verbosity { get; set; }
        public DotNetCoreVerbosity DotNetCoreVerbosity => Enum.TryParse<DotNetCoreVerbosity>(Verbosity.ToString(), out var dotNetCoreVerbosity) ? dotNetCoreVerbosity : DotNetCoreVerbosity.Minimal;

        private bool? _diagnostic;
        public bool Diagnostic
        {
            get => _diagnostic ?? Verbosity > Verbosity.Normal;
            set => _diagnostic = value;
        }

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
