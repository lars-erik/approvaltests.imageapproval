using System;
using System.Collections.Generic;
using ApprovalTests.Writers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ApprovalTests.ImageApproval
{
    public static class ImageApprovals
    {
        public const double DefaultRatioTolerance = 0.001;
        public const double JustNoticableDifference = 2.3;

        public static void Verify(byte[] bytes, string extensionWithoutDot, double ratioTolerance = DefaultRatioTolerance, double justNoticableTolerance = JustNoticableDifference)
        {
            var writer = new ApprovalBinaryWriter(bytes, extensionWithoutDot);
            var approver = new ImageComparisonApprover(writer, Approvals.GetDefaultNamer(), ratioTolerance, justNoticableTolerance);
            Approvals.Verify(approver);
        }

        public static IDisposable CreateContext()
        {
            return new Context();
        }

        public static Comparison Current
        {
            get
            {
                lock (LockObj)
                {
                    if (ContextStack.Count == 0)
                    {
                        return new Context().Comparison;
                    }

                    return ContextStack.Peek().Comparison;
                }
            }
        }

        private static readonly Stack<Context> ContextStack = new Stack<Context>();
        private static readonly object LockObj = new object();

        internal class Context : IDisposable
        {
            internal Comparison Comparison { get; } = new Comparison();

            internal Context()
            {
                lock (LockObj)
                {
                    ContextStack.Push(this);
                }
            }

            public void Dispose()
            {
                lock (LockObj)
                {
                    Comparison?.Dispose();
                    ContextStack.Pop();
                }
            }
        }

        public class Comparison : IDisposable
        {
            private ImageComparer comparer;

            public bool Blur { get; set; } = false;
            public string OutputPath { get; set; } = "TestOutput";

            public bool CreateColorDiff { get; set; } = true;
            public bool CreateCie76Diff { get; set; } = true;
            public DiffStats Statistics { get; internal set; }
            public Image<Rgba32> ColorDiff { get; internal set; }
            public Image<Rgba32> Cie76Diff { get; internal set; }
            public Image<Rgba32> ApprovedOriginal => comparer.ApprovedOriginal;
            public Image<Rgba32> ReceivedOriginal => comparer.ReceivedOriginal;
            public Image<Rgba32> ApprovedCompared => comparer.ApprovedCompared;
            public Image<Rgba32> ReceivedCompared => comparer.ReceivedCompared;

            public void Compare(string approvedPath, string receivedPath, double ratioTolerance)
            {
                Compare(() => CreateComparerByPath(approvedPath, receivedPath, ratioTolerance));
            }

            public void Compare(byte[] approvedBytes, byte[] receivedBytes, double ratioTolerance)
            {
                Compare(() => CreateComparerByBytes(approvedBytes, receivedBytes, ratioTolerance));
            }

            private void Compare(Action createComparer)
            {
                createComparer();

                Statistics = comparer.CompareImages();
            }

            public Image<Rgba32> BuildCIE76Diff()
            {
                return comparer.BuildCIE76Diff(100);
            }

            public Image<Rgba32> BuildColorDiff()
            {
                return comparer.BuildDiff();
            }

            private void CreateComparerByPath(string approvedPath, string receivedPath, double ratioTolerance)
            {
                if (Blur) // TODO: Blur setting
                {
                    comparer = new ImageComparer(approvedPath, receivedPath, ratioTolerance, JustNoticableDifference, x => { x.BoxBlur(2); });
                }
                else
                {
                    comparer = new ImageComparer(approvedPath, receivedPath, ratioTolerance);
                }
            }

            private void CreateComparerByBytes(byte[] approvedBytes, byte[] receivedBytes, double ratioTolerance)
            {
                if (Blur) // TODO: Blur setting
                {
                    comparer = new ImageComparer(approvedBytes, receivedBytes, ratioTolerance, JustNoticableDifference, x => { x.BoxBlur(2); });
                }
                else
                {
                    comparer = new ImageComparer(approvedBytes, receivedBytes, ratioTolerance);
                }
            }

            internal void Clear()
            {
                Dispose();
                comparer = null;

                CreateColorDiff = true;
                CreateCie76Diff = true;
                Statistics = null;
                ColorDiff = null;
                Cie76Diff = null;
            }

            public void Dispose()
            {
                ColorDiff?.Dispose();
                Cie76Diff?.Dispose();
                comparer?.Dispose();
            }
        }
    }
}
