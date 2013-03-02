using Xunit;
using NSubstitute;

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

    public class WorkflowTests
    {
        [Fact]
        public void ExecutesWithoutBusinessRule()
        {
            var model = new CounterModel();
            new CounterIncrementWorkflow().Execute(model);
            Assert.Equal(1, model.Counter);
        }

        [Fact]
        public void ValidatesPreBusinessRuleAndExecutes()
        {
            var model = new CounterModel();

            var preRule = Substitute.For<IBusinessRule<CounterModel>>();
            preRule.IsSatisfied(Arg.Any<CounterModel>()).Returns(true);

            new CounterIncrementWorkflow(preRule).Execute(model);
            
            Assert.Equal(1, model.Counter);          
        }

        [Fact]
        public void ThrowsErrorOnInvalidPreBusinessRuleAndDoesNotExecute()
        {
            var model = new CounterModel();

            var preRule = Substitute.For<IBusinessRule<CounterModel>>();
            preRule.IsSatisfied(Arg.Any<CounterModel>()).Returns(false);

            Assert.Throws(typeof(BusinessRuleException), () => new CounterIncrementWorkflow(preRule).Execute(model));
            Assert.Equal(0, model.Counter);
        }
    }
}
