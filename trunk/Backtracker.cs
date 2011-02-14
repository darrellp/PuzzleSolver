using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolver
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	
	/// This is the heart of PuzzleSolver.  It tries using all the rules until none of them apply any
	/// more and then invokes standard backtracking.  Each time it makes a guess, it tries the rules
	/// again to determine the implications.  If the current board turns out to be impossible to
	/// solve, it backtracks. 
	/// </summary>
	///
	/// <remarks>	Darrellp, 2/14/2011. </remarks>
	///
	/// ### <typeparam name="TPs">	The type of partial solution we apply to. </typeparam>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public class Backtracker<TPs> where TPs : IPartialSolution
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Return the first solution we can find if any and gather backtracking reasons. </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="ps">		Partial solution so far. </param>
		/// <param name="es">		Expert System to apply. </param>
		/// <param name="psFinal">	[out] Final solution. </param>
		/// <param name="lstbti">	[out] Reasons for inferences. </param>
		///
		/// <returns>	True if a solution was found, else false. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		static public bool FSolve(TPs ps, ExpertSystem<TPs> es, out TPs psFinal, out List<BackTrackInfo> lstbti)
		{
			// Set up
			var lstpsSolutions = new List<TPs>();
			lstbti = new List<BackTrackInfo>();
			psFinal = default(TPs);

			// Use SearchForMultipleSolutions to search for precisely one solution
			FSearchForMultipleSolutions(ps, es, lstbti, lstpsSolutions, 1);

			// Did we find a solution?
			if (lstpsSolutions.Count == 1)
			{
				// Put it into our out parameter and return true
				psFinal = lstpsSolutions[0];
				return true;
			}
			// No solution found - return false
			return false;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Return the first solution we can find if any - don't gather backtracking reasons. </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="ps">		Partial solution so far. </param>
		/// <param name="es">		Expert System to apply. </param>
		/// <param name="psFinal">	[out] Final solution. </param>
		///
		/// <returns>	True if a solution was found, else false. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		static public bool FSolve(TPs ps, ExpertSystem<TPs> es, out TPs psFinal)
		{
			List<BackTrackInfo> lstbti;
			return FSolve(ps, es, out psFinal, out lstbti);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	See if there is exactly one unique solution. </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="ps">	Partial solution so far. </param>
		/// <param name="es">	Expert System to apply. </param>
		///
		/// <returns>	true if exactly one solution exists, false otherwise. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// 
		static public bool FUnique(TPs ps, ExpertSystem<TPs> es)
		{
			var lstpsSolutions = new List<TPs>();
			var lstbti = new List<BackTrackInfo>();

			FSearchForMultipleSolutions((TPs)ps.Clone(), es, lstbti, lstpsSolutions, 2);
			return lstpsSolutions.Count == 1;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Searches for a fixed count of solutions.  Returns as many up to that count as it can find. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="ps">				The puzzle. </param>
		/// <param name="es">				The expert system. </param>
		/// <param name="lstbti">			List of backtrack info. </param>
		/// <param name="lstpsSolutions">	The list to place the found solutions in. </param>
		/// <param name="csln">				The desired count of solutions. </param>
		///
		///<returns>	True if we need to keep searching, false if there's no reason for more searching. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		static private bool FSearchForMultipleSolutions(
			TPs ps, ExpertSystem<TPs> es,
			ICollection<BackTrackInfo> lstbti,
			ICollection<TPs> lstpsSolutions,
			int csln)
		{
			// Is an expert system available?
			if (es != null)
			{
				// Apply rules and gather reasons
			    List<ReasonRulePair> lstrrp;
			    var fImpossible = es.FApply(ps, out lstrrp);

				// Are we keeping backtrack reasons?
				if (lstbti != null)
				{
					// Add that we applied the expert system instead of backtracking
					lstbti.Add(new BackTrackInfo(BacktrackReason.ExpertSystemApplication, lstrrp, null));
				}

				// Did we reach an impossible situation?
				if (!fImpossible)
				{
					// Are we keeping backtracing reasons?
					if (lstbti != null)
					{
						// Add that we detected an impossible board
						lstbti.Add(new BackTrackInfo(BacktrackReason.RuleDetectedImpossibility, null, null));
					}
					// No possible solutions on this branch - Search other branches
					return true;
				}
			}

			// Did we solve it with the expert system?
			if (ps.FSolved())
			{
				// Is this is a duplicate of an earlier generated solution?
				if (lstpsSolutions.Any(psCur => ps.IdenticalTo(psCur)))
				{
					// Are we keeping track of backtrack reasons?
					if (lstbti != null)
					{
						// Add Duplicate Solution as the reason for backtracking
						lstbti.Add(new BackTrackInfo(BacktrackReason.DuplicateSolution, null, null));
					}
					return true;
				}

				// New solution - add to the list of solutions
				lstpsSolutions.Add((TPs)ps.Clone());

				// If we've found our goal count of solution, shut the whole operation down
				if (lstpsSolutions.Count == csln)
				{
					// Are we keeping track of backtrack reasons?
					if (lstbti != null)
					{
						// Add goal count reached reason
						lstbti.Add(new BackTrackInfo(BacktrackReason.GoalCountReached, null, null));
					}

					// Return false to stop searching for other solutions
					return false;
				}

				// Are we keeping track of backtrack reasons?
				if (lstbti != null)
				{
					// Add that we found a soln but we need more
					lstbti.Add(new BackTrackInfo(BacktrackReason.GoalReachedButMoreNeeded, null, null));
				}

				// Return true to keep looking for more solutions
				return true;
			}

			// Didn't solve it by rules to start backtracking search

			// Get the list of potential moves from this partial board
			var lstIExtensions = ps.GetIExtensions();

			// Are there no moves from the current position?
			if (lstIExtensions.Count == 0)
			{
				// Are we keeping backtrack reasons?
				if (lstbti != null)
				{
					// Add that we've reached a leaf node
					lstbti.Add(new BackTrackInfo(BacktrackReason.LeafNode, null, null));
				}
				// Return true to keep looking for solutions since none found on this branch
				return true;
			}

			// Assume that we're going to have a choice of moves
			var btr = BacktrackReason.Guess;

			// Is there really only one move available?
			if (lstIExtensions.Count == 1)
			{
				// Indicate that our choice is forced
				btr = BacktrackReason.ForcedChoice;
			}
			else
			{
				// Put the extensions in the most likely order of succeeding heuristically
				lstIExtensions.Sort();
			}

			// Try each extension in turn
			foreach (var ext in lstIExtensions)
			{
				// Are we keeping track of backtrack reasons?
				if (lstbti != null)
				{
					// Add that we're trying this new move
					lstbti.Add(new BackTrackInfo(btr, null, ext));
				}

				// Apply the current extension
				var psCur = (TPs)ps.PsApply(ext, true);

				// Recursively search for a solution with this move
				var fContinueSearch = FSearchForMultipleSolutions(psCur, es, lstbti, lstpsSolutions, csln);

				// If there's no reason for more searching
				if (!fContinueSearch)
				{
					// Return false to tell our caller that
					return false;
				}
			}

			// Still haven't found our goal count - tell our caller to keep looking

			// If we're keeping backtrack reasons
			if (lstbti != null)
			{
				// Add that there are no more moves
				lstbti.Add(new BackTrackInfo(BacktrackReason.NoMoreMoves, null, null));
			}
			return true;
		}
	}
}
