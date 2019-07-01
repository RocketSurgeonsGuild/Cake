namespace Rocket.Surgery.Cake.LcovConverter
{
    /// <summary>
    /// BranchCondition.
    /// </summary>
    class BranchCondition
    {
        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        /// <value>The number.</value>
        public long Number { get; set; }
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; } = "jump";
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="BranchCondition"/> is covered.
        /// </summary>
        /// <value><c>true</c> if covered; otherwise, <c>false</c>.</value>
        public bool Covered { get; set; }
    }
}
