/*
 * The inspiration for this code is the Wikipedia page on the Specification Pattern: 
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
    /// <summary>Interface for Enflow state rules</summary>
    /// <typeparam name="T"></typeparam>
    public interface IStateRule<T>
    {
        Expression<Func<T, bool>> Predicate { get; }
        string Description { get; set; }
        bool IsSatisfied(T candidate);    
    }

    public abstract class StateRule<T> : IStateRule<T>
    {
        public abstract Expression<Func<T, bool>> Predicate { get; }
        public string Description { get; set; }
        public bool IsSatisfied(T candidate) { return Predicate.Compile().Invoke(candidate); }   
      
        /// <summary>
        /// Ensures that parameter expressions refer to the same instance across all expressions that are combined to form the input expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected Expression<Func<T, bool>> ReplaceParameter (Expression expression)
        {
            var parameterExpression = Expression.Parameter(typeof(T));
            var body = new ParameterReplacer(parameterExpression).Visit(expression);
            return Expression.Lambda<Func<T, bool>>(body, parameterExpression);
        }
    }

    /// <summary>Composite state rule where both input rules must be satisfied.</summary>
    /// <typeparam name="T"></typeparam>
    public class AndStateRule<T> : StateRule<T>
    {
        private readonly IStateRule<T> _ruleA;
        private readonly IStateRule<T> _ruleB;

        internal AndStateRule(IStateRule<T> ruleA, IStateRule<T> ruleB)
        {
            _ruleA = ruleA;
            _ruleB = ruleB;
        }

        public override Expression<Func<T, bool>> Predicate
        {
            get { return ReplaceParameter(Expression.And(_ruleA.Predicate.Body, _ruleB.Predicate.Body)); }
        }
    }

    /// <summary>Composite state rule where at least one of the input rules must be satisfied.</summary>
    /// <typeparam name="T"></typeparam>
    public class OrStateRule<T> : StateRule<T>
    {
        private readonly IStateRule<T> _ruleA;
        private readonly IStateRule<T> _ruleB;

        internal OrStateRule(IStateRule<T> ruleA, IStateRule<T> ruleB)
        {
            _ruleA = ruleA;
            _ruleB = ruleB;
        }

        public override Expression<Func<T, bool>> Predicate
        {
            get { return ReplaceParameter(Expression.Or(_ruleA.Predicate.Body, _ruleB.Predicate.Body)); }
        }
    }

    /// <summary>State rule that enforces a logical NOT of the input rule.</summary>
    /// <typeparam name="T"></typeparam>
    public class NotStateRule<T> : StateRule<T>
    {
        private readonly IStateRule<T> _rule;
        internal NotStateRule(IStateRule<T> rule) { _rule = rule; }
        
        public override Expression<Func<T, bool>> Predicate
        {
            get { return ReplaceParameter(Expression.Not(_rule.Predicate.Body)); }
        }
    }

    /// <summary>Facilitates the fluent API for composing state rules from atomic constituents.</summary>
    public static class StateRuleFluentExtensions
    {
        public static IStateRule<T> And<T>(this IStateRule<T> ruleA, IStateRule<T> ruleB)
        {
            return new AndStateRule<T>(ruleA, ruleB);
        }

        public static IStateRule<T> Or<T>(this IStateRule<T> ruleA, IStateRule<T> ruleB)
        {
            return new OrStateRule<T>(ruleA, ruleB);
        }

        public static IStateRule<T> Not<T>(this IStateRule<T> rule)
        {
            return new NotStateRule<T>(rule);
        }

        public static IStateRule<T> Describe<T>(this IStateRule<T> rule, string description)
        {
            rule.Description = description;
            return rule;
        }
    }
}
