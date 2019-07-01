using System.Collections.Generic;

namespace Rocket.Surgery.Cake.LcovConverter
{
    /// <summary>
    /// Package.
    /// </summary>
    class Package
    {
        /// <summary>
        /// Gets or sets the lines total.
        /// </summary>
        /// <value>The lines total.</value>
        public long LinesTotal { get; set; }
        /// <summary>
        /// Gets or sets the lines covered.
        /// </summary>
        /// <value>The lines covered.</value>
        public long LinesCovered { get; set; }
        /// <summary>
        /// Gets the line rate.
        /// </summary>
        /// <value>The line rate.</value>
        public decimal LineRate => Helpers.Percent(LinesTotal, LinesCovered);

        /// <summary>
        /// Gets or sets the branches total.
        /// </summary>
        /// <value>The branches total.</value>
        public long BranchesTotal { get; set; }
        /// <summary>
        /// Gets or sets the branches covered.
        /// </summary>
        /// <value>The branches covered.</value>
        public long BranchesCovered { get; set; }
        /// <summary>
        /// Gets the branch rate.
        /// </summary>
        /// <value>The branch rate.</value>
        public decimal BranchRate => Helpers.Percent(BranchesTotal, BranchesCovered);

        /// <summary>
        /// Gets the classes.
        /// </summary>
        /// <value>The classes.</value>
        public IDictionary<string, Class> Classes { get; } = new Dictionary<string, Class>();
    }
}
