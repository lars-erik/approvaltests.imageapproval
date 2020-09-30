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
        private string approvedPath;

        public NUnitAttachmentReporter()
        {
        }
        
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

        private void AttachAll(string approved)
        {
            approvedPath = approved;
            if (File.Exists(approvedPath))
            {
                TestContext.AddTestAttachment(approvedPath);
            }

            if (!Directory.Exists(ImageApprovals.Current.OutputPath))
            {
                Directory.CreateDirectory(ImageApprovals.Current.OutputPath);
            }

            AttachText(JsonConvert.SerializeObject(ImageApprovals.Current.Statistics, Formatting.Indented), ".diff.json");
            AttachImage(ImageApprovals.Current.ReceivedOriginal, ".screenshot.png");
            
            if (ImageApprovals.Current.Blur)
            { 
                AttachImage(ImageApprovals.Current.ReceivedCompared, ".compared.png");
            }

            if (ImageApprovals.Current.CreateColorDiff)
            {
                var colorDiff = ImageApprovals.Current.BuildColorDiff();
                AttachImage(colorDiff, ".colordiff.png");
            }

            if (ImageApprovals.Current.CreateCie76Diff)
            {
                var cieDiff = ImageApprovals.Current.BuildCIE76Diff();
                AttachImage(cieDiff, ".ciediff.png");
            }
        }

        private void AttachText(string contents, string identifier)
        {
            var path = GetOutputPath(identifier);
            File.WriteAllText(path, contents);
            TestContext.AddTestAttachment(path);
        }

        public void AttachImage(Image<Rgba32> attachment, string identifier)
        {
            if (attachment != null)
            {
                var path = GetOutputPath(identifier);
                attachment.SaveAsPng(path);
                TestContext.AddTestAttachment(path);
            }
        }

        private string GetOutputPath(string identifier)
        {
            var path = approvedPath.Replace(".approved.png", identifier);
            var fileName = Path.GetFileName(path);
            path = Path.Combine(ImageApprovals.Current.OutputPath, fileName);
            return path;
        }
    }
}
