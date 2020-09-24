using System.Collections.Generic;

namespace ApprovalTests.ImageApproval
{
    public class DiffStats
    {
        public Dictionary<bool, int> CountedByNoticable { get; internal set; }
        public Dictionary<bool, double> AveragedByNoticable { get; internal set; }
        public Dictionary<bool, double> SummedByNoticable { get; internal set; }
        public Dictionary<double, int> CountedByCie76Floored { get; internal set; }
        public Dictionary<double, double> AveragedByCie76Floored { get; internal set; }
        public Dictionary<double, double> SummedByCie76Floored { get; internal set; }
        public double MaxCie76 { get; internal set; }
        public double AvgCie76 { get; internal set; }
        public double SumCie76 { get; internal set; }
        public double NoticableRatio { get; internal set; }
        public double ToleratedRato { get; set; }
        public bool ExceedsRatio { get; internal set; }
    }
}