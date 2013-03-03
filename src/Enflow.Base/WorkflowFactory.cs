using System;
using System.Collections.Generic;

namespace Enflow.Base
{
    public interface IWorkflowFactory
    {
        IWorkflow<T> Get<T>(string name) where T : IModel<T>;
    }

    public abstract class WorkflowFactory : IWorkflowFactory
    {
        private readonly Dictionary<string, Func<object>> _registrations = new Dictionary<string, Func<object>>();

        protected void Register<T>(string name, Func<IWorkflow<T>> resolver) where T : IModel<T>
        {
            _registrations.Add(name, resolver);
        }

        public IWorkflow<T> Get<T>(string name) where T : IModel<T>
        {
            try { return _registrations[name].Invoke() as IWorkflow<T>; }
            catch (Exception) { throw new WorkflowFactoryException("Unable to resolve workflow with name: " + name); }           
        }
    }
}
