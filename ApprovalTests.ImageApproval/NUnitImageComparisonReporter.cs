using ApprovalTests.Core;
using ApprovalTests.Namers.StackTraceParsers;
using ApprovalTests.Reporters;
using ApprovalTests.StackTraceParsers;
using NUnit.Framework;

namespace ApprovalTests.ImageApproval
{
    public class NUnitImageComparisonReporter : IEnvironmentAwareReporter
    {
        public static readonly NUnitImageComparisonReporter INSTANCE = new NUnitImageComparisonReporter();

        public NUnitImageComparisonReporter()
        {
        }

        public virtual void Report(string approved, string received)
        {
            QuietReporter.DisplayCommandLineApproval(approved, received);
            Assert.LessOrEqual(
                ImageApprovals.Current.Statistics.NoticableRatio,
                ImageApprovals.Current.Statistics.ToleratedRato,
                $"Noticable image difference ratio of {ImageApprovals.Current.Statistics.NoticableRatio} is over the tolerance of {ImageApprovals.Current.Statistics.ToleratedRato}"
            );
        }

        public virtual bool IsWorkingInThisEnvironment(string forFile)
        {
            return IsFrameworkUsed();
        }

        public bool IsFrameworkUsed()
        {
            return AttributeStackTraceParser.GetFirstFrameForAttribute(Approvals.CurrentCaller, NUnitStackTraceParser.Attribute) != null;
        }

    }
}