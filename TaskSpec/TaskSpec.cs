using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using System.Diagnostics.Contracts;

namespace DotRLGlueCodec.TaskSpec
{
    public class TaskSpec<TStateSpaceType, TActionSpaceType> : TaskSpecBase
    {
        public override Type ObservationSpaceType { get { return typeof(TStateSpaceType); } }

        public override Type ActionSpaceType { get { return typeof(TActionSpaceType); } }

        public override int ObservationSpaceDimensionality { get { return this.observationMinimumValues.Length; } }

        public override int ActionSpaceDimensionality { get { return this.actionMinimumValues.Length; } }

        public IEnumerable<TStateSpaceType> ObservationMinimumValues { get { return this.observationMinimumValues; } }

        public IEnumerable<TStateSpaceType> ObservationMaximumValues { get { return this.observationMaximumValues; } }

        public IEnumerable<TActionSpaceType> ActionMinimumValues { get { return this.actionMinimumValues; } }

        public IEnumerable<TActionSpaceType> ActionMaximumValues { get { return this.actionMaximumValues; } }

        /// <summary>
        /// Makes its own copies of given collections.
        /// </summary>
        public TaskSpec(
            IEnumerable<TStateSpaceType> observationMinimumValues,
            IEnumerable<TStateSpaceType> observationMaximumValues,
            IEnumerable<TActionSpaceType> actionMinimumValues,
            IEnumerable<TActionSpaceType> actionMaximumValues,
            double reinforcementMinimumValue,
            double reinforcementMaximumValue,
            double discountFactor,
            string additionalInformation)
            :base (reinforcementMinimumValue, reinforcementMaximumValue, discountFactor, additionalInformation)
        {
            Contract.Requires(observationMinimumValues.Count() == observationMaximumValues.Count());
            Contract.Requires(actionMinimumValues.Count() == actionMaximumValues.Count());

            this.observationMinimumValues = observationMinimumValues.ToArray();
            this.observationMaximumValues = observationMaximumValues.ToArray();
            this.actionMinimumValues = actionMinimumValues.ToArray();
            this.actionMaximumValues = actionMaximumValues.ToArray();
        }

        public override IEnumerable<object> GetObservationMinimumValues()
        {
            return this.observationMinimumValues.Cast<object>();
        }

        public override IEnumerable<object> GetObservationMaximumValues()
        {
            return this.observationMaximumValues.Cast<object>();
        }

        public override IEnumerable<object> GetActionMinimumValues()
        {
            return this.actionMinimumValues.Cast<object>();
        }

        public override IEnumerable<object> GetActionMaximumValues()
        {
            return this.actionMaximumValues.Cast<object>();
        }

        private TStateSpaceType[] observationMinimumValues;
        private TStateSpaceType[] observationMaximumValues;
        private TActionSpaceType[] actionMinimumValues;
        private TActionSpaceType[] actionMaximumValues;
    }
}
