using System.Collections.Generic;

namespace Rocket.Surgery.Cake.LcovConverter
{
    /// <summary>
    /// Class FileMethod.
    /// </summary>
    class FileMethod
    {
        /// <summary>
        /// Gets or sets the line.
        /// </summary>
        /// <value>The line.</value>
        public long Line { get; set; }
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
