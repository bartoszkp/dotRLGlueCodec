using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotRLGlueCodec.TaskSpec;
using System.IO;

namespace Tests.cs
{
    //VERSION <version-name> PROBLEMTYPE <problem-type> DISCOUNTFACTOR <discount-factor> OBSERVATIONS INTS ([times-to-repeat-this-tuple=1] <min-value> <max-value>)* DOUBLES ([times-to-repeat-this-tuple=1] <min-value> <max-value>)* CHARCOUNT <char-count> ACTIONS INTS ([times-to-repeat-this-tuple=1] <min-value> <max-value>)* DOUBLES ([times-to-repeat-this-tuple=1] <min-value> <max-value>)* CHARCOUNT <char-count> REWARDS (<min-value> <max-value>) EXTRA [extra text of your choice goes here]"; 
    [TestFixture]
    public class TaskSpecParserTests
    {
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.0001 OBSERVATIONS INTS (2 0 2) ACTIONS INTS (0 4) REWARDS (0 10) EXTRA extra string")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE _ DISCOUNTFACTOR 0.0001 OBSERVATIONS INTS (0 2) INTS (0 2) ACTIONS INTS (0 4) REWARDS (0 10) EXTRA extra string")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE _ DISCOUNTFACTOR 0.0001 OBSERVATIONS DOUBLES INTS (0 2) (0 2) ACTIONS INTS (0 4) REWARDS (0 10) EXTRA extra string")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE _ DISCOUNTFACTOR 0.0001 OBSERVATIONS DOUBLES INTS (0 2) INTS (0 2) ACTIONS INTS (0 4) REWARDS (1 0 10) EXTRA extra string")]
        public void ShouldParseCorrectlyTaskSpecs_With_TwoObsIntegerDimensions_And_OneActionIntegerDimension(string input)
        {
            TaskSpecParser parser = new TaskSpecParser();

            TaskSpec<int, int> result = parser.Parse(input) as TaskSpec<int, int>;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ObservationMinimumValues, Is.EqualTo(new[] { 0, 0 }));
            Assert.That(result.ObservationMaximumValues, Is.EqualTo(new[] { 2, 2 }));
            Assert.That(result.ActionMinimumValues, Is.EqualTo(new[] { 0 }));
            Assert.That(result.ActionMaximumValues, Is.EqualTo(new[] { 4 }));
            Assert.That(result.ReinforcementMinimumValue, Is.EqualTo(0));
            Assert.That(result.ReinforcementMaximumValue, Is.EqualTo(10));
            Assert.That(result.AdditionalInformation, Is.EqualTo("extra string"));
            Assert.That(result.DiscountFactor, Is.EqualTo(0.0001));
        }

        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS (1 0 2) ACTIONS INTS (3 0 4) REWARDS (-2 10) EXTRA extra loooong string with space at the end ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS DOUBLES INTS (1 0 2) ACTIONS INTS (3 0 4) REWARDS (-2 10) EXTRA extra loooong string with space at the end ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS (1 0 2) ACTIONS DOUBLES INTS (0 4) (0 4) (0 4) REWARDS (-2 10) EXTRA extra loooong string with space at the end ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS (1 0 2) ACTIONS INTS (3 0 4) REWARDS (1 -2 10) EXTRA extra loooong string with space at the end ")]
        public void ShouldParseCorrectlyTypicalTaskSpecs_With_OneObsIntegerDimension_And_ThreeActionIntegerDimensions(string input)
        {
            TaskSpecParser parser = new TaskSpecParser();

            TaskSpec<int, int> result = parser.Parse(input) as TaskSpec<int, int>;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ObservationMinimumValues, Is.EqualTo(new[] { 0 }));
            Assert.That(result.ObservationMaximumValues, Is.EqualTo(new[] { 2 }));
            Assert.That(result.ActionMinimumValues, Is.EqualTo(new[] { 0, 0, 0 }));
            Assert.That(result.ActionMaximumValues, Is.EqualTo(new[] { 4, 4, 4 }));
            Assert.That(result.ReinforcementMinimumValue, Is.EqualTo(-2));
            Assert.That(result.ReinforcementMaximumValue, Is.EqualTo(10));
            Assert.That(result.AdditionalInformation, Is.EqualTo("extra loooong string with space at the end "));
            Assert.That(result.DiscountFactor, Is.EqualTo(0.4));
        }

        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS DOUBLES (0 2) (-10 100) ACTIONS DOUBLES (-0.1 3.4) (456 9999.9999) REWARDS (0 10) EXTRA    ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS DOUBLES (0 2) (-10 100) ACTIONS DOUBLES (-0.1 3.4) (456 9999.9999) REWARDS (0 10) EXTRA    ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS DOUBLES (0 2) (-10 100) ACTIONS DOUBLES (-0.1 3.4) (456 9999.9999) REWARDS (1 0 10) EXTRA    ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS DOUBLES (0 2) (-10 100) ACTIONS DOUBLES (-0.1 3.4) (456 9999.9999) REWARDS (1 0 10) EXTRA")]
        public void ShouldParseCorrectlyTypicalTaskSpecs_With_TwoObsDoubleDimensions_And_TwoActionDoubleDimensions(string input)
        {
            TaskSpecParser parser = new TaskSpecParser();

            TaskSpec<double, double> result = parser.Parse(input) as TaskSpec<double, double>;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ObservationMinimumValues, Is.EqualTo(new[] { 0.0, -10.0 }));
            Assert.That(result.ObservationMaximumValues, Is.EqualTo(new[] { 2.0, 100.0 }));
            Assert.That(result.ActionMinimumValues, Is.EqualTo(new[] { -0.1, 456 }));
            Assert.That(result.ActionMaximumValues, Is.EqualTo(new[] { 3.4, 9999.9999 }));
            Assert.That(result.ReinforcementMinimumValue, Is.EqualTo(0));
            Assert.That(result.ReinforcementMaximumValue, Is.EqualTo(10));
            Assert.That(result.AdditionalInformation, Is.EqualTo(string.Empty));
        }

        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS DOUBLES (0 2) (-10 100) ACTIONS DOUBLES (-0.1 3.4) (456 9999.9999) REWARDS (0 10)")]
        public void ShouldParseCorrectlyTypicalTaskSpecs_With_TwoObsDoubleDimensions_And_TwoActionDoubleDimensions_WithNoExtraString(string input)
        {
            TaskSpecParser parser = new TaskSpecParser();

            TaskSpec<double, double> result = parser.Parse(input) as TaskSpec<double, double>;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ObservationMinimumValues, Is.EqualTo(new[] { 0.0, -10.0 }));
            Assert.That(result.ObservationMaximumValues, Is.EqualTo(new[] { 2.0, 100.0 }));
            Assert.That(result.ActionMinimumValues, Is.EqualTo(new[] { -0.1, 456 }));
            Assert.That(result.ActionMaximumValues, Is.EqualTo(new[] { 3.4, 9999.9999 }));
            Assert.That(result.ReinforcementMinimumValue, Is.EqualTo(0));
            Assert.That(result.ReinforcementMaximumValue, Is.EqualTo(10));
            Assert.That(result.AdditionalInformation, Is.EqualTo(string.Empty));
        }

        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS (0 2) (-10 100) ACTIONS DOUBLES (-0.1 3.4) (456 9999.9999) REWARDS (0 10) EXTRA    ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS (0 2) (-10 100) ACTIONS DOUBLES (-0.1 3.4) (456 9999.9999) REWARDS (0 10) EXTRA    ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS (0 2) (-10 100) ACTIONS DOUBLES (-0.1 3.4) (456 9999.9999) REWARDS (1 0 10) EXTRA    ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS (0 2) (-10 100) ACTIONS DOUBLES (-0.1 3.4) (456 9999.9999) REWARDS (1 0 10) EXTRA")]
        public void ShouldParseCorrectlyTypicalTaskSpecs_With_TwoObsIntegerDimensions_And_TwoActionDoubleDimensions(string input)
        {
            TaskSpecParser parser = new TaskSpecParser();

            TaskSpec<int, double> result = parser.Parse(input) as TaskSpec<int, double>;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ObservationMinimumValues, Is.EqualTo(new[] { 0, -10 }));
            Assert.That(result.ObservationMaximumValues, Is.EqualTo(new[] { 2, 100 }));
            Assert.That(result.ActionMinimumValues, Is.EqualTo(new[] { -0.1, 456 }));
            Assert.That(result.ActionMaximumValues, Is.EqualTo(new[] { 3.4, 9999.9999 }));
            Assert.That(result.ReinforcementMinimumValue, Is.EqualTo(0));
            Assert.That(result.ReinforcementMaximumValue, Is.EqualTo(10));
            Assert.That(result.AdditionalInformation, Is.EqualTo(string.Empty));
        }

        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS DOUBLES (0 2) (-10 100) ACTIONS DOUBLES INTS (0 3) REWARDS (0 10) EXTRA    ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS DOUBLES (0 2) (-10 100) ACTIONS INTS (0 3) REWARDS (0 10) EXTRA    ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS DOUBLES (0 2) (-10 100) ACTIONS INTS (0 3) REWARDS (1 0 10) EXTRA    ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS DOUBLES (0 2) (-10 100) ACTIONS INTS (0 3) REWARDS (1 0 10) EXTRA")]
        public void ShouldParseCorrectlyTypicalTaskSpecs_With_TwoObsDoubleDimensions_And_OneActionIntegerDimension(string input)
        {
            TaskSpecParser parser = new TaskSpecParser();

            TaskSpec<double, int> result = parser.Parse(input) as TaskSpec<double, int>;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ObservationMinimumValues, Is.EqualTo(new[] { 0.0, -10.0 }));
            Assert.That(result.ObservationMaximumValues, Is.EqualTo(new[] { 2.0, 100.0 }));
            Assert.That(result.ActionMinimumValues, Is.EqualTo(new[] { 0 }));
            Assert.That(result.ActionMaximumValues, Is.EqualTo(new[] { 3 }));
            Assert.That(result.ReinforcementMinimumValue, Is.EqualTo(0));
            Assert.That(result.ReinforcementMaximumValue, Is.EqualTo(10));
            Assert.That(result.AdditionalInformation, Is.EqualTo(string.Empty));
        }


        [TestCase("VERSION wrongversion PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS DOUBLES (0 2) (-10 100) ACTIONS DOUBLES INTS (0 3) REWARDS (0 10) EXTRA    ")]
        [TestCase("VERSION too much tokens PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS DOUBLES (0 2) (-10 100) ACTIONS INTS (0 3) REWARDS (0 10) EXTRA    ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR x OBSERVATIONS DOUBLES (0 2) (-10 100) ACTIONS INTS (0 3) REWARDS (1 0 10) EXTRA    ")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS (0.333 2) (-10 100) ACTIONS INTS (0 3) REWARDS (1 0 10) EXTRA")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 ACTIONS INTS (0 3) REWARDS (1 0 10) EXTRA")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS (0.333 2) (-10 100) REWARDS (1 0 10) EXTRA")]
        [TestCase("VERSION RL-Glue-3.0 PROBLEMTYPE ignored DISCOUNTFACTOR 0.4 OBSERVATIONS INTS (0.333 2) (-10 100) ACTIONS INTS (0 3) EXTRA")]
        [ExpectedException(typeof(InvalidDataException))]
        public void ShouldThrowInvalidDataExceptionForMalformedTaskSpecs(string input)
        {
            (new TaskSpecParser()).Parse(input);
        }
    }
}
