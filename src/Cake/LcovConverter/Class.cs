using System.Collections.Generic;

namespace Rocket.Surgery.Cake.LcovConverter
{
    /// <summary>
    /// .
    /// </summary>
    class Class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Class"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Class(string name)
        {
            Name = name;
        }
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

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
        /// Gets the lines.
        /// </summary>
        /// <value>The lines.</value>
        public IDictionary<long, FileLine> Lines { get; } = new Dictionary<long, FileLine>();
        /// <summary>
        /// Gets the methods.
        /// </summary>
        /// <value>The methods.</value>
        public IDictionary<string, FileMethod> Methods { get; } = new Dictionary<string, FileMethod>();
    }
}
