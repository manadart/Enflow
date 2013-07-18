using System;

namespace Enflow.Base
{
    public class Flowable<T>
    {
        public static readonly Flowable<T> Nothing = new Flowable<T>();

        public T Value { get; private set; }
        public bool HasValue { get; private set; }

        private Flowable()
        {
            HasValue = false;
        }

        public Flowable(T value)
        {
            Value = value;
            HasValue = true;
        }

        public bool Satisfies(IStateRule<T> stateRule)
        {
            return HasValue && stateRule.IsSatisfied(Value);
        }
    }

    public static class FlowableExtensions
    {
        public static Flowable<T> AsFlowable<T>(this T value)
        {
            if (!(value is ValueType) && ReferenceEquals(value, null)) return Flowable<T>.Nothing;
            return new Flowable<T>(value);
        }

        public static Flowable<U> Flow<T, U>(this Flowable<T> flowable, IWorkflow<T, U> workflow)
        {
            return !flowable.HasValue ? Flowable<U>.Nothing : workflow.Execute(flowable.Value).AsFlowable();
        }
    }
}
