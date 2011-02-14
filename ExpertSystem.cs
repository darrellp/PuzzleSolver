using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolver
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	
	/// The ExpertSystem is composed of a set of rules which act on IPartialSolution objects.  It
	/// applies those rules until none apply or until some rule flags the partial solution as
	/// unsolvable. 
	/// </summary>
	///
	/// <remarks>	Darrellp, 2/14/2011. </remarks>
	///
	/// ### <typeparam name="TPs">	The type of the partial solution we're applied to. </typeparam>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public class ExpertSystem<TPs> where TPs : IPartialSolution
	{
		public bool IsKeepingReasons {get; set;}
		readonly List<IRule> _lstIRule;

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Constructor. </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="lstRules">		The list of rules. </param>
		/// <param name="fKeepReasons">	true if we want to gather reasons. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public ExpertSystem(List<IRule> lstRules, bool fKeepReasons)
		{
			_lstIRule = lstRules;
			IsKeepingReasons = fKeepReasons;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Apply the rules of this expert system without keeping reasons. </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="ps">	The partial solution we're being applied to. </param>
		///
		/// <returns>	Returns false if an impossible state is detected, else true. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public bool FApply(TPs ps)
		{
			List<ReasonRulePair> lstrrp;
			return FApply(ps, out lstrrp);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Apply the rules of this expert system. </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="ps">		The partial solution we're being applied to. </param>
		/// <param name="lstrrp">	[out] The list of reasons for rule applications. </param>
		///
		/// <returns>	Returns false if an impossible state is detected, else true. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public bool FApply(TPs ps, out List<ReasonRulePair> lstrrp)
		{
			// Set up
			var fApplied = true;
			lstrrp = IsKeepingReasons ? new List<ReasonRulePair>() : null;

			// As long as some rule is applied, cycle through and try to apply them all
			while (fApplied)
			{
				fApplied = false;
				// For every applicable rule
				foreach (var rl in _lstIRule.Where(rl => rl.FTrigger(ps)))
				{
					List<IReason> lstReason;
					bool fImpossible;

					// Apply the rule
					fApplied = rl.FApply(ps, out lstReason, out fImpossible);

					// Are we at an impossible position?
					if (fImpossible)
					{
						// Are we gathering reasons?
						if (IsKeepingReasons)
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
							// Add the reason(s) for the application
							lstrrp.AddRange(lstReason.Select(reason => new ReasonRulePair(reason, rl)));
						}
						// If we find a rule that applied, then restart at the top
						// since ones at the top (the most "important/useful" ones)
						// may now apply
						break;
					}
				}
			}

			// We've applied as many rules as we can - start a full backtracking search.
			return true;
		}
	}
}
