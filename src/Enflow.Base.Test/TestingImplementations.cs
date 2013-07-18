using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Enflow.Base.Test
{
    public class GuidCount
    {
        public Guid Id { get; private set; }
        public int Counter { get; set; }
        public void Increment() { Counter++; }

        public GuidCount() { Id = Guid.NewGuid(); }
    }

    public class LessThanTenCounterRule : StateRule<GuidCount>
    {
        public override Expression<Func<GuidCount, bool>> Predicate { get { return candidate => candidate.Counter < 10; } }
    }

    public class GuidCountIncrementWorkflow : Workflow<GuidCount>
    {
        public GuidCountIncrementWorkflow() { } 
        public GuidCountIncrementWorkflow(IStateRule<GuidCount> preRule) : base(preRule) { }
        
        protected override GuidCount ExecuteWorkflow(GuidCount candidate) 
        { 
            candidate.Increment();
            return candidate;
        }
    }

    public class GuidCountDuplicateWorkflow : Workflow<GuidCount, List<GuidCount>>
    {
        public GuidCountDuplicateWorkflow() { }
        public GuidCountDuplicateWorkflow(IStateRule<GuidCount> preRule) : base(preRule) { }

        protected override List<GuidCount> ExecuteWorkflow(GuidCount candidate) 
        { 
            return new List<GuidCount> { candidate, new GuidCount { Counter = candidate.Counter } };
        }
    }
}
