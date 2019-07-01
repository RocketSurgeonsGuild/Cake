using System.CodeDom;
using System.Collections.Generic;

namespace Rocket.Surgery.Cake.LcovConverter
{
    /// <summary>
    /// FileLine.
    /// </summary>
    class FileLine
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FileLine"/> is branch.
        /// </summary>
        /// <value><c>true</c> if branch; otherwise, <c>false</c>.</value>
        public bool Branch { get; set; }
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
        /// Gets or sets the hits.
        /// </summary>
        /// <value>The hits.</value>
        public long Hits { get; set; }
        /// <summary>
        /// Gets the conditions.
        /// </summary>
        /// <value>The conditions.</value>
        public List<BranchCondition> Conditions { get; } = new List<BranchCondition>();
    }
}
