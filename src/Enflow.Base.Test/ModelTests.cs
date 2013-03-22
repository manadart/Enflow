using Xunit;

namespace Enflow.Base.Test
{
    public class ModelTests
    {
        [Fact]
        public void CanExecuteChainedWorkflowsViaFlowExtension()
        {
            var workflow = new CounterIncrementWorkflow();
            Assert.Equal(2, new CounterModel().Flow(workflow).Flow(workflow).Counter);
        }
    }
}
