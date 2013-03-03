
namespace Enflow.Base.Test
{
    public class CounterModel : IEnflowModel<CounterModel>
    {
        public int Counter { get; private set; }
        public void Increment() { Counter++; }
    }

    public class CounterIncrementWorkflow : Workflow<CounterModel>
    {
        public CounterIncrementWorkflow() { }
        public CounterIncrementWorkflow(IBusinessRule<CounterModel> preRule) : base(preRule) { }
        protected override void ExecuteWorkflow(CounterModel candidate) { candidate.Increment(); }
    }
}
