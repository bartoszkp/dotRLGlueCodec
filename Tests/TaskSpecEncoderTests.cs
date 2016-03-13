using NUnit.Framework;
using DotRLGlueCodec.TaskSpec;

namespace Tests
{
    [TestFixture]
    public class TaskSpecEncoderTests
    {
        [Test]
        public void ShouldProperlyEncodeTypicalIntIntTaskSpec()
        {
            TaskSpec<int, int> taskSpec = new TaskSpec<int, int>(
                observationMinimumValues: new[] { 0, 0 },
                observationMaximumValues: new[] { 10, 10 },
                actionMinimumValues: new[] { 0 },
                actionMaximumValues: new[] { 4 },
                reinforcementMinimumValue: -4,
                reinforcementMaximumValue: 10.5,
                discountFactor: 0.9,
                additionalInformation: "test");

            string result = (new TaskSpecStringEncoder()).Encode(taskSpec);

            Assert.That(result, Is.EqualTo("VERSION RL-Glue-3.0 PROBLEMTYPE episodic DISCOUNTFACTOR 0.9 OBSERVATIONS INTS (0 10) (0 10) ACTIONS INTS (0 4) REWARDS (-4 10.5) EXTRA test"));
        }

        [Test]
        public void ShouldProperlyEncodeTypicalDoubleIntTaskSpec()
        {
            TaskSpec<double, int> taskSpec = new TaskSpec<double, int>(
                observationMinimumValues: new[] { 0.0 },
                observationMaximumValues: new[] { 10.5 },
                actionMinimumValues: new[] { 0 },
                actionMaximumValues: new[] { 4 },
                reinforcementMinimumValue: -4,
                reinforcementMaximumValue: 10.5,
                discountFactor: 0.9,
                additionalInformation: "test");

            string result = (new TaskSpecStringEncoder()).Encode(taskSpec);

            Assert.That(result, Is.EqualTo("VERSION RL-Glue-3.0 PROBLEMTYPE episodic DISCOUNTFACTOR 0.9 OBSERVATIONS DOUBLES (0 10.5) ACTIONS INTS (0 4) REWARDS (-4 10.5) EXTRA test"));
        }

        [Test]
        public void ShouldProperlyEncodeTypicalIntDoubleTaskSpec()
        {
            TaskSpec<int, double> taskSpec = new TaskSpec<int, double>(
                observationMinimumValues: new[] { 0, 0 },
                observationMaximumValues: new[] { 10, 10 },
                actionMinimumValues: new[] { 0.0, 1.5, 1.3 },
                actionMaximumValues: new[] { 4, 15, 100.5 },
                reinforcementMinimumValue: 100,
                reinforcementMaximumValue: 222,
                discountFactor: 0.9,
                additionalInformation: string.Empty);

            string result = (new TaskSpecStringEncoder()).Encode(taskSpec);

            Assert.That(result, Is.EqualTo("VERSION RL-Glue-3.0 PROBLEMTYPE episodic DISCOUNTFACTOR 0.9 OBSERVATIONS INTS (0 10) (0 10) ACTIONS DOUBLES (0 4) (1.5 15) (1.3 100.5) REWARDS (100 222)"));
        }

        [Test]
        public void ShouldProperlyEncodeTypicalDoubleDoubleTaskSpec()
        {
            TaskSpec<double, double> taskSpec = new TaskSpec<double, double>(
                observationMinimumValues: new[] { 0.0 },
                observationMaximumValues: new[] { 10.5 },
                actionMinimumValues: new[] { 0.0, 1.5, 1.3 },
                actionMaximumValues: new[] { 4, 15, 100.5 },
                reinforcementMinimumValue: 100,
                reinforcementMaximumValue: 222,
                discountFactor: 0.9,
                additionalInformation: string.Empty);

            string result = (new TaskSpecStringEncoder()).Encode(taskSpec);

            Assert.That(result, Is.EqualTo("VERSION RL-Glue-3.0 PROBLEMTYPE episodic DISCOUNTFACTOR 0.9 OBSERVATIONS DOUBLES (0 10.5) ACTIONS DOUBLES (0 4) (1.5 15) (1.3 100.5) REWARDS (100 222)"));
        }
    }
}
