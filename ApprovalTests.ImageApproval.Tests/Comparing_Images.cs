using System.IO;
using ApprovalTests;
using ApprovalTests.Core.Exceptions;
using ApprovalTests.ImageApproval;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.TestFrameworks;
using ApprovalTests.Writers;
using Newtonsoft.Json;
using NUnit.Framework;
using SixLabors.ImageSharp.Formats.Png;

[assembly: UseApprovalSubdirectory("Approvals")]

namespace ApprovalTestContrib.ImageApproval.Tests
{
    [UseReporter(typeof(NUnitAttachmentReporter), typeof(NUnitImageComparisonReporter))]
    [TestFixture]
    public class Comparing_Images
    {
        [Test]
        public void Approves_Equal_Images()
        {
            var imageBytes = GetComparisonImage("ApprovalTestsRocks");
            ImageApprovals.Verify(imageBytes, "png");
        }

        [Test]
        public void Approves_Image_With_Tolerance()
        {
            var imageBytes = GetComparisonImage("ApprovalTestsRocks.Nudged");
            ImageApprovals.Verify(imageBytes, "png", 0.021);
        }

        [Test]
        public void Disapproves_Unequal_Images()
        {
            var imageBytes = GetComparisonImage("ApprovalTestsRocks.Nudged");
            Assert.That(() => ImageApprovals.Verify(imageBytes, "png"), Throws.TypeOf<AssertionException>());
        }

        [Test]
        public void Stores_Color_Diff_In_State()
        {
            var imageBytes = GetComparisonImage("ApprovalTestsRocks.Nudged");
            ImageApprovals.Verify(imageBytes, "png", 0.021);

            using (ApprovalResults.ForScenario("diff"))
            {
                using (var str = new MemoryStream())
                {
                    ImageApprovals.Current.ColorDiff.Save(str, new PngEncoder());
                    str.Seek(0, SeekOrigin.Begin);
                    var bytes = new byte[str.Length];
                    str.Read(bytes, 0, (int)str.Length);
                    Approvals.VerifyBinaryFile(bytes, "png");
                }
            }
        }

        [Test]
        [UseReporter(typeof(NUnitReporter))]
        public void Stores_Json_Diff_In_State()
        {
            var imageBytes = GetComparisonImage("ApprovalTestsRocks.Nudged");
            ImageApprovals.Verify(imageBytes, "png", 0.021);

            Approvals.VerifyJson(JsonConvert.SerializeObject(ImageApprovals.Current.Statistics));
        }

        private byte[] GetComparisonImage(string fileName)
        {
            byte[] result;
            using (var stream = GetType().Assembly
                .GetManifestResourceStream(
                    $"ApprovalTestContrib.ImageApproval.Tests.Comparison_Images.{fileName}.png"))
            {
                result = new byte[stream.Length]; // TODO: Handle too big streams
                stream.Read(result, 0, (int)stream.Length);
            }

            return result;
        }
    }
}