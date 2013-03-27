using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Enflow.Base.Test
{
    public class PositiveCounterRule : StateRule<CounterModel>
    {
        public override bool IsSatisfied(CounterModel candidate) { return candidate.Counter > 0; }
    }

    public class NegativeCounterRule : StateRule<CounterModel>
    {
        public override bool IsSatisfied(CounterModel candidate) { return candidate.Counter < 0; }
    }

    public class StateRuleTests
    {
        [Fact]
        public void SimpleSatisfiedRuleReturnsTrue()
        {
            var model = new CounterModel();
            model.Increment();
            Assert.True(new PositiveCounterRule().IsSatisfied(model));
        }

        [Fact]
        public void NotExtensionEvaluesCorrectly()
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

        [Fact]
        public void OrExtensionEvaluatesCorrectly()
        {
            var model = new CounterModel();
            model.Increment();
            Assert.True(new PositiveCounterRule().Or(new NegativeCounterRule()).IsSatisfied(model));
        }

        [Fact]
        public void AndExtensionEvaluatesCorrectly()
        {
            var model = new CounterModel();
            model.Increment();
            Assert.False(new PositiveCounterRule().And(new NegativeCounterRule()).IsSatisfied(model));
        }

        [Fact]
        public void AsExpressionExtensionFiltersCorrectly()
        {
            var validCandidate = new CounterModel { Counter = 1 };
            var invalidCandidate = new CounterModel { Counter = -1 };

            var candidates = new List<CounterModel> { validCandidate, invalidCandidate }.AsQueryable();
            var filtered = candidates.Where(new PositiveCounterRule().Predicate).ToList();

            Assert.Equal(1, filtered.Count);
            Assert.Equal(validCandidate.Id, filtered[0].Id);
        }
    }
}
