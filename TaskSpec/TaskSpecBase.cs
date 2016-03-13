using System;
using System.Collections.Generic;

namespace DotRLGlueCodec.TaskSpec
{
    public abstract class TaskSpecBase
    {
        public abstract Type ObservationSpaceType { get; }

        public abstract Type ActionSpaceType { get; }

        public abstract int ObservationSpaceDimensionality { get; }

        public abstract int ActionSpaceDimensionality { get; }

        public double ReinforcementMinimumValue { get; private set; }

        public double ReinforcementMaximumValue { get; private set; }

        public double DiscountFactor { get; private set; }

        public string AdditionalInformation { get; private set; }

        protected TaskSpecBase(
            double reinforcementMinimumValue,
            double reinforcementMaximumValue,
            double discountFactor,
            string additionalInformation)
        {
            this.ReinforcementMinimumValue = reinforcementMinimumValue;
            this.ReinforcementMaximumValue = reinforcementMaximumValue;
            this.DiscountFactor = discountFactor;
            this.AdditionalInformation = additionalInformation;
        }

        public abstract IEnumerable<object> GetObservationMinimumValues();

        public abstract IEnumerable<object> GetObservationMaximumValues();

        public abstract IEnumerable<object> GetActionMinimumValues();

        public abstract IEnumerable<object> GetActionMaximumValues();
    }
}
