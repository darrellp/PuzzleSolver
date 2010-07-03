using System.Collections.Generic;

namespace PuzzleSolver
{
	/// <summary>
	/// This is the heart of PuzzleSolver.  It tries using all the rules until none of them apply any more
	/// and then invokes standard backtracking.  Each time it makes a guess, it tries the rules again to
	/// determine the implications.  If the current board turns out to be impossible to solve, it backtracks.
	/// </summary>
	/// <typeparam name="TPs"></typeparam>
	public class Backtracker<TPs> where TPs : IPartialSolution
	{
		/// <summary>
		/// Return the first solution we can find if any
		/// </summary>
		/// <param name="ps">Partial solution so far</param>
		/// <param name="es">Expert System to apply</param>
		/// <param name="psFinal">Final solution</param>
		/// <param name="lstbti">Reasons for inferences</param>
		/// <returns>True if a solution was found, else false</returns>
		static public bool FSolve(TPs ps, ExpertSystem<TPs> es, out TPs psFinal, out List<BackTrackInfo> lstbti)
		{
			List<TPs> lstpsSolutions = new List<TPs>();
			lstbti = new List<BackTrackInfo>();
			psFinal = default(TPs);

			FSearchForMultipleSolutions(ps, es, lstbti, lstpsSolutions, 1, null, BacktrackReason.InitialEntry);
			if (lstpsSolutions.Count == 1)
			{
				psFinal = lstpsSolutions[0];
				return true;
			}
			return false;
		}

		/// <summary>
		/// Return the first solution we can find if any
		/// </summary>
		/// <param name="ps">Partial solution so far</param>
		/// <param name="es">Expert System to apply</param>
		/// <param name="psFinal">Final solution</param>
		/// <returns>True if a solution was found, else false</returns>
		static public bool FSolve(TPs ps, ExpertSystem<TPs> es, out TPs psFinal)
		{
			List<BackTrackInfo> lstbti;
			return FSolve(ps, es, out psFinal, out lstbti);
		}

		// See if there is exactly one unique solution.
		static public bool FUnique(TPs ps, ExpertSystem<TPs> es)
		{
			List<TPs> lstpsSolutions = new List<TPs>();
			List<BackTrackInfo> lstbti = new List<BackTrackInfo>();

			FSearchForMultipleSolutions((TPs)ps.Clone(), es, lstbti, lstpsSolutions, 2, null, BacktrackReason.InitialEntry);
			return lstpsSolutions.Count == 1;
		}

	    /// <summary>
	    /// Searches for a fixed count of solutions.  Returns as many up to that count as it can find.
	    /// </summary>
	    /// <param name="ps">The puzzle</param>
	    /// <param name="es">The expert system</param>
	    /// <param name="lstbti">List of backtrack info</param>
	    /// <param name="lstpsSolutions">The list to place the found solutions in</param>
	    /// <param name="csln">The desired count of solutions</param>
	    /// <param name="iext">Extension used</param>
	    /// <param name="btr">Reason we have to backtrack</param>
	    /// <returns>True if we need to keep searching</returns>
	    static private bool FSearchForMultipleSolutions(
			TPs ps, ExpertSystem<TPs> es, List<BackTrackInfo> lstbti, List<TPs> lstpsSolutions, int csln, IExtension iext, BacktrackReason btr)
		{
	        BacktrackReason btrCall = BacktrackReason.Guess;

			// Use any rules that might apply...
			if (es != null)
			{
			    List<ReasonRulePair> lstrrp;
			    bool fApplied = es.FApply(ps, out lstrrp);
				lstbti.Add(new BackTrackInfo(BacktrackReason.NoBacktrack, lstrrp, iext));

				if (!fApplied)
				{
					if (lstbti != null)
					{
						lstbti.Add(new BackTrackInfo(BacktrackReason.RuleDetectedImpossibility, null, null));
					}
					// If this partial solution is impossible, keep searching for others
					return true;
				}
			}

			// Did we solve it with the rules?
			if (ps.FSolved())
			{
				// See if this is a duplicate of an earlier generated solution
				foreach (TPs psCur in lstpsSolutions)
				{
					// If so, backtrack and keep searching for our goal count
					if (ps.IdenticalTo(psCur))
					{
						if (lstbti != null)
						{
							lstbti.Add(new BackTrackInfo(BacktrackReason.DuplicateSolution, null, null));
						}
						return true;
					}
				}

				// New solution - add to the list of solutions
				lstpsSolutions.Add((TPs)ps.Clone());

				// If we've found our goal count of solution, shut the whole operation down and return failure to
				// stop searching for further solutions
				if (lstpsSolutions.Count == csln)
				{
					if (lstbti != null)
					{
						lstbti.Add(new BackTrackInfo(BacktrackReason.GoalCountReached, null, null));
					}
					return false;
				}

				// If we need more solutions, backtrack and keep searching
				if (lstbti != null)
				{
					lstbti.Add(new BackTrackInfo(BacktrackReason.GoalReachedButMoreNeeded, null, null));
				}
				return true;
			}

			// Didn't solve it by rules to start backtracking search
			List<IExtension> lstIExtensions = ps.GetIExtensions();

			// If there are no more branches in this part of the search, keep trying at our parent node
			if (lstIExtensions.Count == 0)
			{
				if (lstbti != null)
				{
					lstbti.Add(new BackTrackInfo(BacktrackReason.LeafNode, null, null));
				}
				return true;
			}

			if (lstbti.Count == 1)
			{
				btrCall = BacktrackReason.ForcedChoice;
			}
			else
			{
				// Put the extensions in the most likely order of succeeding heuristically
				lstIExtensions.Sort();
			}

			// Try each extension in turn
			foreach (IExtension ext in lstIExtensions)
			{
				TPs psCur = (TPs)ps.PsApply(ext, true);

				// If we've found our goal count of solutions, just return up the tree
				if (!FSearchForMultipleSolutions(psCur, es, lstbti, lstpsSolutions, csln, ext, btrCall))
				{
					// False says we're done searching
					return false;
				}
			}

			// Still haven't found our goal count - keep looking back at our parent node
			if (lstbti != null)
			{
				lstbti.Add(new BackTrackInfo(BacktrackReason.NoMoreExtensions, null, null));
			}
			return true;
		}
	}

	public enum BacktrackReason
	{
		InitialEntry,
		ForcedChoice,
		Guess,
		NoBacktrack,
		LeafNode,
		NoMoreExtensions,
		RuleDetectedImpossibility,
		GoalReachedButMoreNeeded,
		GoalCountReached,
		DuplicateSolution
	}

	public class BackTrackInfo
	{
		public BacktrackReason BackTrackReason { get; private set; }
		public List<ReasonRulePair> ReasonList { get; private set; }
		public IExtension Extension { get; private set; }

		public BackTrackInfo(BacktrackReason btReason, List<ReasonRulePair> lstrrp, IExtension ext)
		{
			ReasonList = lstrrp;
			Extension = ext;
			BackTrackReason = btReason;
		}

		public override string ToString()
		{
			return BackTrackReason + (Extension == null ? "" : Extension.ToString());
		}
	}
}
