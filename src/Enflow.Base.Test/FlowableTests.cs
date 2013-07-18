using System.Linq;
using Xunit;

namespace Enflow.Base.Test
{
    public class FlowableTests
    {
        [Fact]
        public void NullAsFlowableIsEmpty()
        {
            string wrapped = null;
            var flowableString = wrapped.AsFlowable();
            Assert.False(flowableString.HasValue);
        }

        [Fact]
        public void CanExecuteChainedWorkflowsOfSameTypeViaFlow()
        {
            var workflow = new GuidCountIncrementWorkflow();
            Assert.Equal(2, new GuidCount().AsFlowable().Flow(workflow).Flow(workflow).Value.Counter);
        }

        [Fact]
        public void CanExecuteChainedWorkflowsOfDifferentTypeViaFlow()
        {
            var incWorkflow = new GuidCountIncrementWorkflow();
            var dupWorkflow = new GuidCountDuplicateWorkflow();

            var results = new GuidCount().AsFlowable().Flow(incWorkflow).Flow(incWorkflow).Flow(dupWorkflow).Value;
            Assert.Equal(2, results.Count);
            Assert.True(results.All(r => r.Counter == 2));
        }

        [Fact]
        public void CanTestStateRuleViaExtension()
        {
            Assert.True(new GuidCount().AsFlowable().Flow(new GuidCountIncrementWorkflow()).Satisfies(new LessThanTenCounterRule()));
        }
    }
}
