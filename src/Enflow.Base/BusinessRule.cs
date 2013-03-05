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

namespace Enflow.Base
{
    /// <summary>Interface for Enflow business rules. Can only be applied to core types.</summary>
    /// <typeparam name="T"></typeparam>
    public interface IBusinessRule<in T> where T : IModel<T>
    {
        string Description { get; set; }
        bool IsSatisfied(T candidate);       
    }

    public abstract class BusinessRule<T> : IBusinessRule<T> where T : IModel<T>
    {
        public string Description { get; set; }
        public abstract bool IsSatisfied(T candidate);
    }

    /// <summary>Composite business rule where both input rules must be satisfied.</summary>
    /// <typeparam name="T"></typeparam>
    public class AndBusinessRule<T> : BusinessRule<T> where T : IModel<T> 
    {
        private readonly IBusinessRule<T> _ruleA;
        private readonly IBusinessRule<T> _ruleB;

        internal AndBusinessRule(IBusinessRule<T> ruleA, IBusinessRule<T> ruleB)
        {
            _ruleA = ruleA;
            _ruleB = ruleB;
        }

        public override bool IsSatisfied(T candidate) { return _ruleA.IsSatisfied(candidate) && _ruleB.IsSatisfied(candidate); }
    }

    /// <summary>Composite business rule where at least one of the input rules must be satisfied.</summary>
    /// <typeparam name="T"></typeparam>
    public class OrBusinessRule<T> : BusinessRule<T> where T : IModel<T> 
    {
        private readonly IBusinessRule<T> _ruleA;
        private readonly IBusinessRule<T> _ruleB;

        internal OrBusinessRule(IBusinessRule<T> ruleA, IBusinessRule<T> ruleB)
        {
            _ruleA = ruleA;
            _ruleB = ruleB;
        }

        public override bool IsSatisfied(T candidate) { return _ruleA.IsSatisfied(candidate) || _ruleB.IsSatisfied(candidate); }
    }

    /// <summary>Business rule that enforces a logical NOT of the input rule.</summary>
    /// <typeparam name="T"></typeparam>
    public class NotBusinessRule<T> : BusinessRule<T> where T : IModel<T> 
    {
        private readonly IBusinessRule<T> _rule;
        internal NotBusinessRule(IBusinessRule<T> rule) { _rule = rule; }
        public override bool IsSatisfied(T candidate) { return !_rule.IsSatisfied(candidate); }
    }

    /// <summary>Facilitates the fluent API for composing business rules from atomic constituents.</summary>
    public static class BusinessRuleFluentExtensions
    {
        public static IBusinessRule<T> And<T>(this IBusinessRule<T> ruleA, IBusinessRule<T> ruleB) where T : IModel<T> 
        {
            return new AndBusinessRule<T>(ruleA, ruleB);
        }

        public static IBusinessRule<T> Or<T>(this IBusinessRule<T> ruleA, IBusinessRule<T> ruleB) where T : IModel<T> 
        {
            return new OrBusinessRule<T>(ruleA, ruleB);
        }

        public static IBusinessRule<T> Not<T>(this IBusinessRule<T> rule) where T : IModel<T> 
        {
            return new NotBusinessRule<T>(rule);
        }

        public static IBusinessRule<T> Describe<T>(this IBusinessRule<T> rule, string description) where T : IModel<T>
        {
            rule.Description = description;
            return rule;
        }
    }
}
