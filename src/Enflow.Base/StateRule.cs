/*
 * The basis for this code is the Wikipedia page on the Specification Pattern: 
 *  http://en.wikipedia.org/wiki/Specification_pattern
 * 
 * It is shared under the Creative Commons Attribution-ShareAlike License.
 * The terms of use are stated here: 
 *  http://wikimediafoundation.org/wiki/Terms_of_Use
 * The license terms are available here: 
 *  http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * An in depth look at the Specification Pattern is available in the excellent paper by Martin Fowler and Eric Evans: 
 *  http://martinfowler.com/apsupp/spec.pdf
 */

using System;
using System.Linq.Expressions;

namespace Enflow.Base
{
    /// <summary>Interface for Enflow business rules. Can only be applied to core types.</summary>
    /// <typeparam name="T"></typeparam>
    public interface IStateRule<T> where T : IModel<T>
    {
        string Description { get; set; }
        bool IsSatisfied(T candidate);
        Expression<Func<T, bool>> Predicate { get; }
    }

    public abstract class StateRule<T> : IStateRule<T> where T : IModel<T>
    {
        public string Description { get; set; }
        public abstract bool IsSatisfied(T candidate);
        public Expression<Func<T, bool>> Predicate { get { return c => IsSatisfied(c); } }
    }

    /// <summary>Composite business rule where both input rules must be satisfied.</summary>
    /// <typeparam name="T"></typeparam>
    public class AndStateRule<T> : StateRule<T> where T : IModel<T> 
    {
        private readonly IStateRule<T> _ruleA;
        private readonly IStateRule<T> _ruleB;

        internal AndStateRule(IStateRule<T> ruleA, IStateRule<T> ruleB)
        {
            _ruleA = ruleA;
            _ruleB = ruleB;
        }

        public override bool IsSatisfied(T candidate) { return _ruleA.IsSatisfied(candidate) && _ruleB.IsSatisfied(candidate); }
    }

    /// <summary>Composite business rule where at least one of the input rules must be satisfied.</summary>
    /// <typeparam name="T"></typeparam>
    public class OrStateRule<T> : StateRule<T> where T : IModel<T> 
    {
        private readonly IStateRule<T> _ruleA;
        private readonly IStateRule<T> _ruleB;

        internal OrStateRule(IStateRule<T> ruleA, IStateRule<T> ruleB)
        {
            _ruleA = ruleA;
            _ruleB = ruleB;
        }

        public override bool IsSatisfied(T candidate) { return _ruleA.IsSatisfied(candidate) || _ruleB.IsSatisfied(candidate); }
    }

    /// <summary>Business rule that enforces a logical NOT of the input rule.</summary>
    /// <typeparam name="T"></typeparam>
    public class NotStateRule<T> : StateRule<T> where T : IModel<T> 
    {
        private readonly IStateRule<T> _rule;
        internal NotStateRule(IStateRule<T> rule) { _rule = rule; }
        public override bool IsSatisfied(T candidate) { return !_rule.IsSatisfied(candidate); }
    }

    /// <summary>Facilitates the fluent API for composing business rules from atomic constituents.</summary>
    public static class StateRuleFluentExtensions
    {
        public static IStateRule<T> And<T>(this IStateRule<T> ruleA, IStateRule<T> ruleB) where T : IModel<T> 
        {
            return new AndStateRule<T>(ruleA, ruleB);
        }

        public static IStateRule<T> Or<T>(this IStateRule<T> ruleA, IStateRule<T> ruleB) where T : IModel<T> 
        {
            return new OrStateRule<T>(ruleA, ruleB);
        }

        public static IStateRule<T> Not<T>(this IStateRule<T> rule) where T : IModel<T> 
        {
            return new NotStateRule<T>(rule);
        }

        public static IStateRule<T> Describe<T>(this IStateRule<T> rule, string description) where T : IModel<T>
        {
            rule.Description = description;
            return rule;
        }
    }
}
