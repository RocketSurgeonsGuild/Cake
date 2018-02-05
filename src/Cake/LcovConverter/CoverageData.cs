using System;
using System.Collections.Generic;

namespace Rocket.Surgery.Cake.LcovConverter
{
    class CoverageData
    {
        public IDictionary<string, Package> Packages { get; } = new Dictionary<string, Package>();
        public long LinesTotal { get; set; }
        public long LinesCovered { get; set; }
        public decimal LineRate => Helpers.Percent(LinesTotal, LinesCovered);

        public long BranchesTotal { get; set; }
        public long BranchesCovered { get; set; }
        public decimal BranchRate => Helpers.Percent(BranchesTotal, BranchesCovered);

        public long Timestamp => Convert.ToInt32(DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
    }
}
