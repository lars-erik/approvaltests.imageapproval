using System.IO;
using ApprovalTests.Core;
using ApprovalTests.Core.Exceptions;

namespace ApprovalTests.ImageApproval
{
    public class ImageComparisonApprover : IApprovalApprover
    {
        private readonly IApprovalWriter writer;
        private readonly IApprovalNamer namer;
        private readonly double ratioTolerance;
        private readonly double justNoticableDifference;
        private string approvedPath;
        private string receivedPath;
        private ApprovalException failure;

        public ImageComparisonApprover(
            IApprovalWriter writer, 
            IApprovalNamer namer, 
            double ratioTolerance = ImageApprovals.DefaultRatioTolerance, 
            double justNoticableDifference = ImageApprovals.JustNoticableDifference)
        {
            this.writer = writer;
            this.namer = namer;
            this.ratioTolerance = ratioTolerance;
            this.justNoticableDifference = justNoticableDifference;
        }

        public virtual bool Approve()
        {
            var basename = Path.Combine(namer.SourcePath, namer.Name);
            approvedPath = Path.GetFullPath(writer.GetApprovalFilename(basename));
            receivedPath = Path.GetFullPath(writer.GetReceivedFilename(basename));
            receivedPath = writer.WriteReceivedFile(receivedPath);

            failure = Approve(approvedPath, receivedPath);
            return failure == null;
        }

        public void Fail()
        {
            throw failure;
        }

        public void ReportFailure(IApprovalFailureReporter reporter)
        {
            reporter.Report(approvedPath, receivedPath);
        }

        public void CleanUpAfterSuccess(IApprovalFailureReporter reporter)
        {
            File.Delete(receivedPath);
            var withCleanUp = reporter as IApprovalReporterWithCleanUp;
            withCleanUp?.CleanUp(approvedPath, receivedPath);
        }

        public ApprovalException Approve(string approvedPath, string receivedPath)
        {
            if (!File.Exists(approvedPath))
            {
                return new ApprovalMissingException(receivedPath, approvedPath);
            }

            Compare(approvedPath, receivedPath);

            if (ImageApprovals.Current.Statistics.ExceedsRatio)
            {
                return new ApprovalMismatchException(receivedPath, approvedPath);
            }

            return null;
        }

        public void Compare(string approvedPath, string receivedPath)
        {
            ImageApprovals.Current.Compare(approvedPath, receivedPath, ratioTolerance);

        }
    }

}
