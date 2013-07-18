using System.Collections.Generic;
using Xunit;
using NSubstitute;

namespace Enflow.Base.Test
{
    public class WorkflowTests
    {
        [Fact]
        public void ExecutesWithoutBusinessRule()
        {
            var model = new GuidCount();
            new GuidCountIncrementWorkflow().Execute(model);
            Assert.Equal(1, model.Counter);
        }

        [Fact]
        public void ValidatesPreBusinessRuleAndExecutes()
        {
            var model = new GuidCount();

            var preRule = Substitute.For<IStateRule<GuidCount>>();
            preRule.IsSatisfied(Arg.Any<GuidCount>()).Returns(true);

            new GuidCountIncrementWorkflow(preRule).Execute(model);
            
            Assert.Equal(1, model.Counter);          
        }

        [Fact]
        public void TypeModWorkflowExecutesCorrectly()
        {
            var model = new GuidCount();

            var preRule = Substitute.For<IStateRule<GuidCount>>();
            preRule.IsSatisfied(Arg.Any<GuidCount>()).Returns(true);

            var result = new GuidCountDuplicateWorkflow(preRule).Execute(model);

            Assert.IsType<List<GuidCount>>(result);
            Assert.Equal(2, result.Count);     
        }

        [Fact]
        public void ThrowsErrorOnInvalidPreBusinessRuleAndDoesNotExecute()
        {
            var model = new GuidCount();

            var preRule = Substitute.For<IStateRule<GuidCount>>();
            preRule.IsSatisfied(Arg.Any<GuidCount>()).Returns(false);

            const string description = "This mock rule must be satisfied.";
            preRule.Description = description;
            
            var message = Assert.Throws(typeof(StateRuleException), () => new GuidCountIncrementWorkflow(preRule).Execute(model)).Message;
            Assert.Equal(description, message);
            Assert.Equal(0, model.Counter);
        } 
    }
}
