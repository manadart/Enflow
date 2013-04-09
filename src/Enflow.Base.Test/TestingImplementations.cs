using System;

namespace Enflow.Base.Test
{
    public class CounterModel : IModel<CounterModel>
    {
        public Guid Id { get; private set; }
        public int Counter { get; set; }
        public void Increment() { Counter++; }

        public CounterModel() { Id = Guid.NewGuid(); }
    }

    public class CounterIncrementWorkflow : Workflow<CounterModel>
    {
        public CounterIncrementWorkflow() { }
        public CounterIncrementWorkflow(IStateRule<CounterModel> preRule) : base(preRule) { }
        protected override void ExecuteWorkflow(CounterModel candidate) { candidate.Increment(); }
    }
}
