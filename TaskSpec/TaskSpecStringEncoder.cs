using System.Globalization;
using System.Linq;
using System.Text;

namespace DotRLGlueCodec.TaskSpec
{
    public class TaskSpecStringEncoder : ITaskSpecStringEncoder
    {
        public string Encode(TaskSpecBase taskSpec)
        {
            StringBuilder result = new StringBuilder();

            result.Append("VERSION RL-Glue-3.0 PROBLEMTYPE episodic DISCOUNTFACTOR ");
            result.Append(string.Format(CultureInfo.InvariantCulture, "{0}", taskSpec.DiscountFactor));
            result.Append(" ");

            result.Append("OBSERVATIONS ");
            result.Append(TaskSpecParser.TypeToRangeDescription(taskSpec.ObservationSpaceType) + " ");
            foreach (string range in Enumerable.Zip(
                taskSpec.GetObservationMinimumValues(),
                taskSpec.GetObservationMaximumValues(),
                (min, max) => string.Format(CultureInfo.InvariantCulture, "({0} {1}) ", min, max)))
            {
                result.Append(range);
            }

            result.Append("ACTIONS ");
            result.Append(TaskSpecParser.TypeToRangeDescription(taskSpec.ActionSpaceType) + " ");

            foreach (string range in Enumerable.Zip(
                taskSpec.GetActionMinimumValues(),
                taskSpec.GetActionMaximumValues(),
                (min, max) => string.Format(CultureInfo.InvariantCulture, "({0} {1}) ", min, max)))
            {
                result.Append(range);
            }

            result.Append(string.Format(CultureInfo.InvariantCulture, "REWARDS ({0} {1})", taskSpec.ReinforcementMinimumValue, taskSpec.ReinforcementMaximumValue));

            if (!string.IsNullOrEmpty(taskSpec.AdditionalInformation))
            {
                result.Append(" EXTRA " + taskSpec.AdditionalInformation);
            }

            return result.ToString();
        }
    }
}
