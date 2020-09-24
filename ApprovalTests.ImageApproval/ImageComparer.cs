using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ApprovalTests.ImageApproval
{
    public class ImageComparer : IDisposable
    {
        private readonly double ratioTolerance;
        private readonly double justNoticableDifference;
        private Image<Rgba32> receivedCompared;
        private Image<Rgba32> approvedCompared;

        private Image<Rgba32> receivedOriginal = null;
        private Image<Rgba32> approvedOriginal = null;

        public ImageComparer(string approvedPath, string receivedPath, double ratioTolerance, double justNoticableDifference = 2.3,
            Action<IImageProcessingContext> mutation = null)
            : this(File.ReadAllBytes(receivedPath), File.ReadAllBytes(approvedPath), ratioTolerance, justNoticableDifference, mutation)
        {
        }

        public ImageComparer(byte[] approvedBytes, byte[] receivedBytes, double ratioTolerance, double justNoticableDifference = 2.3,
            Action<IImageProcessingContext> mutation = null)
        {
            this.ratioTolerance = ratioTolerance;
            this.justNoticableDifference = justNoticableDifference;
            receivedOriginal = Image.Load(receivedBytes);
            approvedOriginal = Image.Load(approvedBytes);

            if (mutation != null)
            {
                receivedCompared = receivedOriginal.Clone();
                receivedCompared.Mutate(mutation);
                approvedCompared = approvedOriginal.Clone();
                approvedCompared.Mutate(mutation);
            }
            else
            {
                receivedCompared = receivedOriginal;
                approvedCompared = approvedOriginal;
            }
        }

        public Image<Rgba32> ReceivedCompared => receivedCompared;

        public Image<Rgba32> ApprovedCompared => approvedCompared;

        public Image<Rgba32> ReceivedOriginal => receivedOriginal;

        public Image<Rgba32> ApprovedOriginal => approvedOriginal;

        public void Dispose()
        {
            receivedCompared?.Dispose();
            approvedCompared?.Dispose();
            receivedOriginal?.Dispose();
            approvedOriginal?.Dispose();
        }

        public DiffStats CompareImages()
        {
            var comparer = new CIE76Analyzer(justNoticableDifference);
            var equalities = comparer.Analyze(approvedCompared, receivedCompared);
            var scores = comparer.Score(approvedCompared, receivedCompared);

            var bools = new bool[(equalities.GetUpperBound(0) + 1) * (equalities.GetUpperBound(1) + 1)];
            for (var y = 0; y <= equalities.GetUpperBound(1); y++)
            {
                for (var x = 0; x <= equalities.GetUpperBound(0); x++)
                {
                    bools[y * (equalities.GetUpperBound(0) + 1) + x] = equalities[x, y];
                }
            }

            var noticableCounts = bools.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            var groupedByDiff = scores.GroupBy(x => Math.Floor(x));

            var noticableRatio = noticableCounts.ContainsKey(true) ? (double)noticableCounts[true] / noticableCounts[false] : 0;
            var exceedsRatio = ratioTolerance <= noticableRatio;
            var result = new DiffStats
            {
                NoticableRatio = noticableRatio,
                ToleratedRato = ratioTolerance,
                ExceedsRatio = exceedsRatio,

                CountedByNoticable = noticableCounts,
                SummedByNoticable = new[] { true, false }.ToDictionary(x => x,
                    noticable => scores.Where(s => (s >= ratioTolerance) == noticable).DefaultIfEmpty(0).Sum()),
                AveragedByNoticable = new[] { true, false }.ToDictionary(x => x,
                    noticable => scores.Where(s => (s >= ratioTolerance) == noticable).DefaultIfEmpty(0).Average()),

                CountedByCie76Floored = groupedByDiff.ToDictionary(x => x.Key, x => x.Count()),
                AveragedByCie76Floored = groupedByDiff.ToDictionary(x => x.Key, x => x.Average()),
                SummedByCie76Floored = groupedByDiff.ToDictionary(x => x.Key, x => x.Sum()),

                MaxCie76 = scores.Max(),
                AvgCie76 = scores.Average(),
                SumCie76 = scores.Sum()
            };
            return result;
        }

        public Image<Rgba32> BuildDiff()
        {
            var width = Math.Max(receivedCompared.Width, approvedCompared.Width);
            var height = Math.Max(receivedCompared.Height, approvedCompared.Height);

            var diff = new Image<Rgba32>(width, height);

            for (var y = 0; y < receivedCompared.Height; y++)
            {
                for (var x = 0; x < receivedCompared.Width; x++)
                {
                    var approvedPx = y >= approvedCompared.Height ? new Rgba32(255, 255, 255) : approvedCompared[x, y];
                    var receivedPx = y >= receivedCompared.Height ? new Rgba32(255, 255, 255) : receivedCompared[x, y];
                    var diffColor = new Rgba32(
                        (byte)Math.Abs(receivedPx.R - approvedPx.R),
                        (byte)Math.Abs(receivedPx.G - approvedPx.G),
                        (byte)Math.Abs(receivedPx.B - approvedPx.B),
                        255
                    );
                    diff[x, y] = diffColor;
                }
            }

            return diff;
        }

        public Image<Rgba32> BuildCIE76Diff(double maxDiff)
        {
            var width = Math.Max(receivedCompared.Width, approvedCompared.Width);
            var height = Math.Max(receivedCompared.Height, approvedCompared.Height);

            var diff = new Image<Rgba32>(width, height);

            for (var y = 0; y < receivedCompared.Height; y++)
            {
                for (var x = 0; x < receivedCompared.Width; x++)
                {
                    var firstLab =
                        CIE76Analyzer.CIELab.FromRGBA(y >= approvedCompared.Height
                            ? new Rgba32(255, 255, 255)
                            : approvedCompared[x, y]);
                    var secondLab =
                        CIE76Analyzer.CIELab.FromRGBA(y >= receivedCompared.Height
                            ? new Rgba32(255, 255, 255)
                            : receivedCompared[x, y]);

                    var score = Math.Sqrt(Math.Pow(secondLab.L - firstLab.L, 2) +
                                          Math.Pow(secondLab.a - firstLab.a, 2) +
                                          Math.Pow(secondLab.b - firstLab.b, 2));

                    var whiteness = (int)Math.Floor(score / maxDiff * 255);

                    diff[x, y] = new Rgba32(whiteness, whiteness, whiteness);
                }
            }

            return diff;
        }

        
    }
}