using Xunit;

namespace Enflow.Base.Test
{
    public class PositiveCounterRule : BusinessRule<CounterModel>
    {
        public override bool IsSatisfied(CounterModel candidate) { return candidate.Counter > 0; }
    }

    public class BusinessRuleTests
    {
        [Fact]
        public void SimpleSatisfiedRuleReturnsTrue()
        {
            var model = new CounterModel();
            model.Increment();
            Assert.True(new PositiveCounterRule().IsSatisfied(model));
        }

        [Fact]
        public void NotExtensionReturnsFalse()
        {
            var model = new CounterModel();
            model.Increment();
            Assert.False(new PositiveCounterRule().Not().IsSatisfied(model));
        }

        [Fact]
        public void DescribeExtensionSetsDescription()
        {
            const string ruleDescription = "This rule enforces a counter > 0.";
            Assert.Equal(ruleDescription, new PositiveCounterRule().Describe(ruleDescription).Description);
        }
    }
}
