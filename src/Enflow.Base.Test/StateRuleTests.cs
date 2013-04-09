using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Enflow.Base.Test
{
    public class PositiveCounterRule : StateRule<CounterModel>
    {
        public override Expression<Func<CounterModel, bool>> Predicate { get { return candidate => candidate.Counter > 0; } }
    }

    public class NegativeCounterRule : StateRule<CounterModel>
    {
        public override Expression<Func<CounterModel, bool>> Predicate { get { return candidate => candidate.Counter < 0; } }
    }

    public class LessThanTenCounterRule : StateRule<CounterModel>
    {
        public override Expression<Func<CounterModel, bool>> Predicate { get { return candidate => candidate.Counter < 10; } }
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
        public void NotExtensionEvaluesCorrectly()
        {
            var model = new CounterModel();
            model.Increment();
            Assert.False(new PositiveCounterRule().Not().IsSatisfied(model));
        }

        [Fact]
        public void PredicateFiltersCorrectly()
        {
            var validCandidate = new CounterModel { Counter = 1 };
            var invalidCandidate = new CounterModel { Counter = -1 };

            var candidates = new List<CounterModel> { validCandidate, invalidCandidate }.AsQueryable();
            var filtered = candidates.Where(new PositiveCounterRule().Predicate).ToList();

            Assert.Equal(1, filtered.Count);
            Assert.Equal(validCandidate.Id, filtered[0].Id);
        }

        [Fact]
        public void AndPredicateFiltersCorrectly()
        {
            var validCandidate = new CounterModel { Counter = 1 };
            var invalidCandidate = new CounterModel { Counter = -1 };

            var candidates = new List<CounterModel> { validCandidate, invalidCandidate }.AsQueryable();
            var filtered = candidates.Where(new PositiveCounterRule().And(new LessThanTenCounterRule()).Predicate).ToList();

            Assert.Equal(1, filtered.Count);
            Assert.Equal(validCandidate.Id, filtered[0].Id);
        }

        [Fact]
        public void OrPredicateFiltersCorrectly()
        {
            var validCandidate = new CounterModel { Counter = 1 };
            var invalidCandidate = new CounterModel { Counter = 20 };

            var candidates = new List<CounterModel> { validCandidate, invalidCandidate }.AsQueryable();
            var filtered = candidates.Where(new NegativeCounterRule().Or(new LessThanTenCounterRule()).Predicate).ToList();

            Assert.Equal(1, filtered.Count);
            Assert.Equal(validCandidate.Id, filtered[0].Id);
        }

        [Fact]
        public void NotPredicateFiltersCorrectly()
        {
            var validCandidate = new CounterModel { Counter = 1 };
            var invalidCandidate = new CounterModel { Counter = -1 };

            var candidates = new List<CounterModel> { validCandidate, invalidCandidate }.AsQueryable();
            var filtered = candidates.Where(new NegativeCounterRule().Not().Predicate).ToList();

            Assert.Equal(1, filtered.Count);
            Assert.Equal(validCandidate.Id, filtered[0].Id);
        }
    }
}
