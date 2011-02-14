using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolver
{
	/// <summary>
	/// The ExpertSystem is composed of a set of rules which act on
	/// IPartialSolution objects.  It applies those rules until none
	/// apply or until some rule flags the partial solution as unsolvable.
	/// </summary>
	/// <typeparam name="TPs"></typeparam>
	public class ExpertSystem<TPs> where TPs : IPartialSolution
	{
		public bool IsKeepingReasons {get; set;}
		readonly List<IRule> _lstIRule;

		public ExpertSystem(List<IRule> lstRules, bool fKeepReasons)
		{
			_lstIRule = lstRules;
			IsKeepingReasons = fKeepReasons;
		}

		public bool FApply(TPs ps)
		{
			List<ReasonRulePair> lstrrp;
			return FApply(ps, out lstrrp);
		}

		// Returns false if an impossible state is detected
		public bool FApply(TPs ps, out List<ReasonRulePair> lstrrp)
		{
			// Set up
			bool fApplied = true;
			lstrrp = IsKeepingReasons ? new List<ReasonRulePair>() : null;

			// As long as some rule is applied, cycle through and try to apply them all
			while (fApplied)
			{
				fApplied = false;
				foreach (IRule rl in _lstIRule)
				{
					// If the rule fires
					if (rl.FTrigger(ps))
					{
						List<IReason> lstReason;
						bool fImpossible;

						// Apply the rule
						fApplied = rl.FApply(ps, out lstReason, out fImpossible);

						// Are we at an impossible position?
						if (fImpossible)
						{
							// Are we gathering reasons?
							if (IsKeepingReasons && lstReason != null)
							{
								// Add the reason we failed
								lstrrp.AddRange(lstReason.Select(reason => new ReasonRulePair(reason, rl)));
							}
							// The rule detected an unsolveable partial solution - return false
							return false;
						}
						if (fApplied)
						{
							// Are we gathering reasons?
							if (IsKeepingReasons && lstReason != null)
							{
								// Add the reason for the application
								lstrrp.AddRange(lstReason.Select(reason => new ReasonRulePair(reason, rl)));
							}
							// If we find a rule that applied, then go to the top of the rules
							// list since ones at the top (the most "important/useful" ones)
							// may now apply
							break;
						}
					}
				}
			}

			// We've applied as many rules as we can - start a full backtracking search.
			return true;
		}
	}

	public struct ReasonRulePair
	{
		public IReason Reason;
		internal IRule Rule;

		public ReasonRulePair(IReason reasonParm, IRule ruleParm)
		{
			Reason = reasonParm;
			Rule = ruleParm;
		}
	}
}
