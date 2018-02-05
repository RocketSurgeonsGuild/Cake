namespace Rocket.Surgery.Cake.LcovConverter
{
    static class Helpers
    {
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
