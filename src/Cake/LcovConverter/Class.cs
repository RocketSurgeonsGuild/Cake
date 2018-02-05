using System.Collections.Generic;

namespace Rocket.Surgery.Cake.LcovConverter
{
    class Class
    {
        public Class(string name)
        {
            Name = name;
        }
        public string Name { get; }

        public long LinesTotal { get; set; }
        public long LinesCovered { get; set; }
        public decimal LineRate => Helpers.Percent(LinesTotal, LinesCovered);

        public long BranchesTotal { get; set; }
        public long BranchesCovered { get; set; }
        public decimal BranchRate => Helpers.Percent(BranchesTotal, BranchesCovered);

        public IDictionary<long, FileLine> Lines { get; } = new Dictionary<long, FileLine>();
        public IDictionary<string, FileMethod> Methods { get; } = new Dictionary<string, FileMethod>();
    }
}
