using System.Collections.Generic;

namespace Rocket.Surgery.Cake.LcovConverter
{
    class Package
    {
        public long LinesTotal { get; set; }
        public long LinesCovered { get; set; }
        public decimal LineRate => Helpers.Percent(LinesTotal, LinesCovered);

        public long BranchesTotal { get; set; }
        public long BranchesCovered { get; set; }
        public decimal BranchRate => Helpers.Percent(BranchesTotal, BranchesCovered);

        public IDictionary<string, Class> Classes { get; } = new Dictionary<string, Class>();
    }
}
