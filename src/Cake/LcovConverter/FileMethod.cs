using System.Collections.Generic;

namespace Rocket.Surgery.Cake.LcovConverter
{
    class FileMethod
    {
        public long Line { get; set; }
        public long Hits { get; set; }
        public List<BranchCondition> Conditions { get; } = new List<BranchCondition>();
    }
}
