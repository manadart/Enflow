using Xunit;

namespace Enflow.Base.Test
{
    public static class Workflows
    {
        public const string Counter = "counter";
    }

    public class TestFactory : WorkflowFactory
    {
        public TestFactory() { Register(Workflows.Counter, () => new CounterIncrementWorkflow()); }
    }

    public class WorkflowFactoryTests
    {
        [Fact]
        public void CanRegisterAndResolve()
        {
            Assert.IsType(typeof(CounterIncrementWorkflow), new TestFactory().Get<CounterModel>(Workflows.Counter));
        }

        [Fact]
        public void ThrowsUnresolvedException()
        {
            Assert.Throws<WorkflowFactoryException>(() => new TestFactory().Get<CounterModel>("non registered name"));
        }
    }
}
