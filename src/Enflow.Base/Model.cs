using System;
using System.Collections.Generic;

namespace Enflow.Base
{
    /// <summary>Marker interface for core types in the Enflow system.</summary>
    public interface IModel<T> { }

    public static class ModelFluentExtensions
    {
        /// <summary>Passes this object through the input workflow.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="candidate"></param>
        /// <param name="workflow"></param>
        /// <returns></returns>
        public static T Flow<T>(this T candidate, IWorkflow<T> workflow) where T : IModel<T>
        {
            workflow.Execute(candidate);
            return candidate;
        }
    }
}