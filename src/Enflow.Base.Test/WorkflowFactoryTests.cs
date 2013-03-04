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

    public class Dummy : IModel<Dummy> { }

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
            var message = Assert.Throws<WorkflowFactoryException>(() => new TestFactory().Get<CounterModel>("non registered name")).Message;
            Assert.Equal("Unable to resolve workflow with name: non registered name", message);
        }

        [Fact]
        public void ThrowsExceptionForNonMatchingGenericArgument()
        {
            var message = Assert.Throws<WorkflowFactoryException>(() => new TestFactory().Get<Dummy>(Workflows.Counter)).Message;
            Assert.Equal("Wrong generic argument supplied for workflow with name: " + Workflows.Counter, message);
        }
    }
}
