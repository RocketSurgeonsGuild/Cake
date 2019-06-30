namespace Rocket.Surgery.Cake.LcovConverter
{
    /// <summary>
    /// Class Helpers.
    /// </summary>
    static class Helpers
    {
        /// <summary>
        /// Percents the specified total.
        /// </summary>
        /// <param name="total">The total.</param>
        /// <param name="covered">The covered.</param>
        /// <returns>System.Decimal.</returns>
        public static decimal Percent(long total, long covered)
        {
            if (total == 0)
            {
                return 0.0M;
            }
            return (decimal)covered / (decimal)total;
        }
    }
}
