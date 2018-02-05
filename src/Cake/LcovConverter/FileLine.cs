using System.CodeDom;
using System.Collections.Generic;

namespace Rocket.Surgery.Cake.LcovConverter
{
    class FileLine
    {
        public bool Branch { get; set; }
        public long BranchesTotal { get; set; }
        public long BranchesCovered { get; set; }
        public decimal BranchRate => Helpers.Percent(BranchesTotal, BranchesCovered);
        public long Hits { get; set; }
        public List<BranchCondition> Conditions { get; } = new List<BranchCondition>();
    }
}
