using System.IO;
using ApprovalTests.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ApprovalTests.ImageApproval
{
    public class NUnitAttachmentReporter : IEnvironmentAwareReporter, IApprovalReporterWithCleanUp
    {
        public static readonly NUnitAttachmentReporter INSTANCE = new NUnitAttachmentReporter();

        private bool failed = false;
        public virtual bool IsWorkingInThisEnvironment(string forFile)
        {
            return true;
        }

        public void Report(string approved, string received)
        {
            failed = true;
            AttachAll(approved);
        }

        public void CleanUp(string approved, string received)
        {
            if (failed)
            {
                failed = false;
                return;
            }

            AttachAll(approved);
        }

        private static void AttachAll(string approved)
        {
            if (File.Exists(approved))
            {
                TestContext.AddTestAttachment(approved);
            }

            AttachText(JsonConvert.SerializeObject(ImageApprovals.Current.Statistics, Formatting.Indented), ".diff.json");
            AttachImage(ImageApprovals.Current.ReceivedOriginal, ".screenshot.png");
            AttachImage(ImageApprovals.Current.ReceivedCompared, ".compared.png");
            AttachImage(ImageApprovals.Current.ColorDiff, ".colordiff.png");
            AttachImage(ImageApprovals.Current.Cie76Diff, ".ciediff.png");
        }

        private static void AttachText(string contents, string identifier)
        {
            File.WriteAllText(TestContext.CurrentContext.Test.FullName + identifier, contents);
            TestContext.AddTestAttachment(TestContext.CurrentContext.Test.FullName + identifier);
        }

        public static void AttachImage(Image<Rgba32> attachment, string identifier)
        {
            if (attachment != null)
            {
                attachment.SaveAsPng(TestContext.CurrentContext.Test.FullName + identifier);
                TestContext.AddTestAttachment(TestContext.CurrentContext.Test.FullName + identifier);
            }
        }
    }
}
