using System;
using System.Collections.Generic;
using System.Text;

namespace PuzzleSolver
{
	/// <summary>
	/// The ExpertSystem is composed of a set of rules which act on
	/// IPartialSolution objects.  It applies those rules until none
	/// apply or until some rule flags the partial solution as unsolvable.
	/// </summary>
	/// <typeparam name="PS"></typeparam>
	public class ExpertSystem<PS> where PS : IPartialSolution
	{
		public bool IsKeepingReasons {get; set;}
		List<IRule> _lstIRule;

		public ExpertSystem(List<IRule> lstRules, bool fKeepReasons)
		{
			_lstIRule = lstRules;
			IsKeepingReasons = fKeepReasons;
		}

		public bool FApply(PS ps)
		{
			List<ReasonRulePair> lstrrp;
			return FApply(ps, out lstrrp);
		}

		// Returns false if an impossible state is detected
		public bool FApply(PS ps, out List<ReasonRulePair> lstrrp)
		{
			bool fApplied = true;
			bool fImpossible = false;
			lstrrp = IsKeepingReasons ? new List<ReasonRulePair>() : null;

			// As long as some rule applied, cycle through and try to apply them all
			while (fApplied)
			{
				fApplied = false;
				foreach (IRule rl in _lstIRule)
				{
					if (rl.FTrigger(ps))
					{
						List<IReason> lstReason;

						fApplied = rl.FApply(ps, out lstReason, out fImpossible);
						if (fImpossible)
						{
							// Keep the information on the application if desired
							if (IsKeepingReasons && lstReason != null)
							{
								foreach(IReason reason in lstReason)
                                {
									lstrrp.Add(new ReasonRulePair(reason, rl));
                                }
							}
							// The rule detected an unsolveable partial solution
							return false;
						}
						if (fApplied)
						{
							// Keep the information on the application if desired
							if (IsKeepingReasons && lstReason != null)
							{
								foreach(IReason reason in lstReason)
                                {
									lstrrp.Add(new ReasonRulePair(reason, rl));
                                }
							}
							// If we find a rule that applied, then go to the top of the rules
							// list since ones at the top (the most "important/useful" ones)
							// may now apply
							break;
						}
					}
				}
			}

			// We've applied as many rules as we can without ever detecting an impossible
			// situation - that's all we can do right now - start a full backtracking search.
			return true;
		}
	}

	public struct ReasonRulePair
	{
		public IReason reason;
		internal IRule rule;

		public ReasonRulePair(IReason reasonParm, IRule ruleParm)
		{
			reason = reasonParm;
			rule = ruleParm;
		}
	}
}
