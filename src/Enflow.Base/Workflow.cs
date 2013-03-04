
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

        /// <summary>Validates the pre-condition business rule and executes the workflow logic.</summary>
        /// <param name="candidate"></param>
        public virtual void Execute(T candidate)
        {
            ValidateBusinessRule(candidate, _preBusinessRule); 
            ExecuteWorkflow(candidate);
            // Todo: implement post-condition rule validation.
        }

        /// <summary>Workflow logic *Not* including business rule validation.</summary>
        /// <param name="candidate"></param>
        protected abstract void ExecuteWorkflow(T candidate);

        private static void ValidateBusinessRule(T candidate, IBusinessRule<T> rule)
        {
            if (rule == null) return;
            if (!rule.IsSatisfied(candidate)) throw new BusinessRuleException();
        }
    }
}
