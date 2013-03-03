
namespace Enflow.Base
{
    /// <summary>Interface for transitioning Enflow model instances through workflows.</summary>
    /// <typeparam name="T"></typeparam>
    public interface IWorkflow<in T> where T : IModel<T>
    {
        void Execute(T candidate);
    }

    public abstract class Workflow<T> : IWorkflow<T> where T : IModel<T>
    {
        private readonly IBusinessRule<T> _preBusinessRule;

        protected Workflow() { }
        protected Workflow(IBusinessRule<T> preBusinessRule) { _preBusinessRule = preBusinessRule; }

        public virtual void Execute(T candidate)
        {
            ValidateBusinessRule(candidate, _preBusinessRule); 
            ExecuteWorkflow(candidate);
            // Todo: validate post rule.
        }

        protected abstract void ExecuteWorkflow(T candidate);

        // Todo: implement message in business rule for failure scenario.
        private static void ValidateBusinessRule(T candidate, IBusinessRule<T> rule)
        {
            if (rule == null) return;
            if (!rule.IsSatisfied(candidate)) throw new BusinessRuleException();
        }
    }
}
