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
    /// <summary>
    /// Class Settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="verbosity">The verbosity.</param>
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

        /// <summary>
        /// Gets the x unit.
        /// </summary>
        /// <value>The x unit.</value>
        public XUnitSettings XUnit { get; } = new XUnitSettings();
        /// <summary>
        /// Gets the pack.
        /// </summary>
        /// <value>The pack.</value>
        public PackSettings Pack { get; } = new PackSettings();
        /// <summary>
        /// Gets the coverage.
        /// </summary>
        /// <value>The coverage.</value>
        public CoverageSettings Coverage { get; } = new CoverageSettings();
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public GitVersion Version { get; }
        /// <summary>
        /// Gets the environment.
        /// </summary>
        /// <value>The environment.</value>
        public Dictionary<string, string> Environment { get; }
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public string Configuration { get; }
        /// <summary>
        /// Gets the verbosity.
        /// </summary>
        /// <value>The verbosity.</value>
        public Verbosity Verbosity { get; }

        /// <summary>
        /// Gets the ms build verbosity.
        /// </summary>
        /// <value>The ms build verbosity.</value>
        public Verbosity MsBuildVerbosity
        {
            get
            {
                if (Diagnostic)
                {
                    return Verbosity;
                }

                if (Verbosity == Verbosity.Normal)
                {
                    return Verbosity.Minimal;
                }

                return Verbosity;
            }
        }

        /// <summary>
        /// Gets the dot net core verbosity.
        /// </summary>
        /// <value>The dot net core verbosity.</value>
        public DotNetCoreVerbosity DotNetCoreVerbosity
        {
            get
            {
                switch (MsBuildVerbosity)
                {
                    case Verbosity.Quiet: return DotNetCoreVerbosity.Quiet;
                    case Verbosity.Minimal: return DotNetCoreVerbosity.Minimal;
                    case Verbosity.Normal: return DotNetCoreVerbosity.Normal;
                    case Verbosity.Verbose: return DotNetCoreVerbosity.Detailed;
                    case Verbosity.Diagnostic: return DotNetCoreVerbosity.Diagnostic;
                }
                return DotNetCoreVerbosity.Minimal;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Settings"/> is diagnostic.
        /// </summary>
        /// <value><c>true</c> if diagnostic; otherwise, <c>false</c>.</value>
        public bool Diagnostic { get; set; }

        /// <summary>
        /// Class XUnitSettings.
        /// </summary>
        public class XUnitSettings
        {
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="XUnitSettings"/> is enabled.
            /// </summary>
            /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
            public bool Enabled { get; set; } = true;
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="XUnitSettings"/> is build.
            /// </summary>
            /// <value><c>true</c> if build; otherwise, <c>false</c>.</value>
            public bool Build { get; set; } = true;
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="XUnitSettings"/> is restore.
            /// </summary>
            /// <value><c>true</c> if restore; otherwise, <c>false</c>.</value>
            public bool Restore { get; set; } = true;
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="XUnitSettings"/> is shadow.
            /// </summary>
            /// <value><c>true</c> if shadow; otherwise, <c>false</c>.</value>
            public bool Shadow { get; set; }
        }

        /// <summary>
        /// Class CoverageSettings.
        /// </summary>
        public class CoverageSettings
        {
            /// <summary>
            /// Gets or sets the attribute filters.
            /// </summary>
            /// <value>The attribute filters.</value>
            public ISet<string> AttributeFilters { get; set; } = new HashSet<string>
            {
                "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
                "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute"
            };

            /// <summary>
            /// Gets or sets the filters.
            /// </summary>
            /// <value>The filters.</value>
            public ISet<string> Filters { get; set; } = new HashSet<string>
            {
                "+:Rocket.*", "-:*.Tests"
            };

            /// <summary>
            /// Applies the specified settings.
            /// </summary>
            /// <param name="settings">The settings.</param>
            /// <returns>DotCoverCoverSettings.</returns>
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

        /// <summary>
        /// Class PackSettings.
        /// </summary>
        public class PackSettings
        {
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="PackSettings"/> is enabled.
            /// </summary>
            /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
            public bool Enabled { get; set; } = true;
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="PackSettings"/> is build.
            /// </summary>
            /// <value><c>true</c> if build; otherwise, <c>false</c>.</value>
            public bool Build { get; set; } = false;
            /// <summary>
            /// Gets or sets a value indicating whether [include symbols].
            /// </summary>
            /// <value><c>true</c> if [include symbols]; otherwise, <c>false</c>.</value>
            public bool IncludeSymbols { get; set; } = true;
            /// <summary>
            /// Gets or sets a value indicating whether [include source].
            /// </summary>
            /// <value><c>true</c> if [include source]; otherwise, <c>false</c>.</value>
            public bool IncludeSource { get; set; } = true;
            /// <summary>
            /// Gets or sets the exclude paths.
            /// </summary>
            /// <value>The exclude paths.</value>
            public List<string> ExcludePaths { get; set; } = new List<string>();
        }
    }
}
