using System.Drawing;

namespace ApprovalTests.ImageApproval
{
    // Code from https://github.com/richclement/ImageDiff
    internal class ExactMatchAnalyzer
    {
        public bool[,] Analyze(Bitmap first, Bitmap second)
        {
            var diff = new bool[first.Width, first.Height];
            for (var x = 0; x < first.Width; x++)
            {
                for (var y = 0; y < first.Height; y++)
                {
                    var firstPixel = first.GetPixel(x, y);
                    var secondPixel = second.GetPixel(x, y);
                    if (firstPixel != secondPixel)
                    {
                        diff[x, y] = true;
                    }
                }
            }
            return diff;
        }
    }
}