namespace Rocket.Surgery.Cake.LcovConverter
{
    class BranchCondition
    {
        public long Number { get; set; }
        public string Type { get; } = "jump";
        public bool Covered { get; set; }
    }
}
