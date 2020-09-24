using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ApprovalTests.ImageApproval
{
    // Code from https://github.com/richclement/ImageDiff
    internal class CIE76Analyzer
    {
        private double JustNoticeableDifference { get; set; }

        public CIE76Analyzer(double justNoticeableDifference)
        {
            JustNoticeableDifference = justNoticeableDifference;
        }

        public bool[,] Analyze(Image<Rgba32> approvedImg, Image<Rgba32> receivedImg)
        {
            var diff = new bool[approvedImg.Width, approvedImg.Height];
            for (var x = 0; x < approvedImg.Width; x++)
            {
                for (var y = 0; y < approvedImg.Height; y++)
                {
                    var index = y * approvedImg.Width + x;
                    var firstLab = CIELab.FromRGBA(approvedImg[x, y]);
                    var secondLab = y >= receivedImg.Height ? CIELab.FromRGBA(new Rgba32(255, 255, 255)) : CIELab.FromRGBA(receivedImg[x, y]);

                    var score = Math.Sqrt(Math.Pow(secondLab.L - firstLab.L, 2) +
                                          Math.Pow(secondLab.a - firstLab.a, 2) +
                                          Math.Pow(secondLab.b - firstLab.b, 2));

                    diff[x, y] = (score >= JustNoticeableDifference);
                }
            }
            return diff;
        }

        public double[] Score(Image<Rgba32> approvedImg, Image<Rgba32> receivedImg)
        {
            var diff = new double[approvedImg.Width * approvedImg.Height];

            for (var x = 0; x < approvedImg.Width; x++)
            {
                for (var y = 0; y < approvedImg.Height; y++)
                {
                    var firstLab = CIELab.FromRGBA(approvedImg[x, y]);
                    var secondLab = y >= receivedImg.Height ? CIELab.FromRGBA(new Rgba32(255, 255, 255)) : CIELab.FromRGBA(receivedImg[x, y]);

                    var score = Math.Sqrt(Math.Pow(secondLab.L - firstLab.L, 2) +
                                          Math.Pow(secondLab.a - firstLab.a, 2) +
                                          Math.Pow(secondLab.b - firstLab.b, 2));

                    diff[y * approvedImg.Width + x] = score;
                }
            }
            return diff;
        }

        internal struct CIELab
        {
            public double L { get; set; }
            public double a { get; set; }
            public double b { get; set; }

            public CIELab(double l, double a, double b)
                : this()
            {
                this.L = l;
                this.a = a;
                this.b = b;
            }

            public static CIELab FromRGBA(Rgba32 color)
            {
                return FromCIExyz(CIExyz.FromRGB(color));
            }

            public static CIELab FromCIExyz(CIExyz xyzColor)
            {
                var transformedX = Transformxyz(xyzColor.x / CIExyz.RefX);
                var transformedY = Transformxyz(xyzColor.y / CIExyz.RefY);
                var transformedZ = Transformxyz(xyzColor.z / CIExyz.RefZ);

                var L = 116.0 * transformedY - 16;
                var a = 500.0 * (transformedX - transformedY);
                var b = 200.0 * (transformedY - transformedZ);

                return new CIELab(L, a, b);
            }

            private static double Transformxyz(double t)
            {
                return ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : ((7.787 * t) + (16.0 / 116.0)));
            }
        }

        internal struct CIExyz
        {

            public const double RefX = 95.047;
            public const double RefY = 100.000;
            public const double RefZ = 108.883;

            public double x { get; set; }
            public double y { get; set; }
            public double z { get; set; }

            public CIExyz(double x, double y, double z)
                : this()
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public static CIExyz FromRGB(Rgba32 color)
            {
                // normalize red, green, blue values
                var rLinear = color.R / 255.0;
                var gLinear = color.G / 255.0;
                var bLinear = color.B / 255.0;

                // convert to a sRGB form
                var r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (1.055), 2.4) : (rLinear / 12.92);
                var g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (1.055), 2.4) : (gLinear / 12.92);
                var b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (1.055), 2.4) : (bLinear / 12.92);

                // converts
                return new CIExyz((r * 0.4124 + g * 0.3576 + b * 0.1805),
                    (r * 0.2126 + g * 0.7152 + b * 0.0722),
                    (r * 0.0193 + g * 0.1192 + b * 0.9505));
            }
        }
    }
}