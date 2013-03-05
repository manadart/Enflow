using Xunit;
using NSubstitute;

namespace Enflow.Base.Test
{
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

            const string description = "This mock rule must be satisfied.";
            preRule.Description = description;
            
            var message = Assert.Throws(typeof(BusinessRuleException), () => new CounterIncrementWorkflow(preRule).Execute(model)).Message;
            Assert.Equal(description, message);
            Assert.Equal(0, model.Counter);
        }
    }
}
