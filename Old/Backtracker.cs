using System;
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
		/// <param name="bti">		[out] Reasons for inferences. </param>
		///
		///<returns>	True if a solution was found, else false. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		static public bool FSolve(TPs ps, ExpertSystem<TPs> es, out TPs psFinal, out BacktrackInfo bti)
		{
			// Set up
			var lstpsSolutions = new List<TPs>();
			bti = new BacktrackInfo(BacktrackReason.InitialEntry, null);
			psFinal = default(TPs);

			// Use SearchForMultipleSolutions to search for precisely one solution
			FSearchForMultipleSolutions(ps, es, bti, lstpsSolutions, 1);

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
			BacktrackInfo bti;
			return FSolve(ps, es, out psFinal, out bti);
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

		static public bool FUnique(TPs ps, ExpertSystem<TPs> es)
		{
			var lstpsSolutions = new List<TPs>();
			var bti = new BacktrackInfo(BacktrackReason.InitialEntry, null);

			FSearchForMultipleSolutions((TPs)ps.Clone(), es, bti, lstpsSolutions, 2);
			return lstpsSolutions.Count == 1;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Searches for a fixed count of solutions.  Returns as many up to that count as it can find. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="ps">				The puzzle up to this point. </param>
		/// <param name="es">				The expert system. </param>
		/// <param name="bti">				Backtrack info. </param>
		/// <param name="lstpsSolutions">	The list to place the found solutions in. </param>
		/// <param name="csln">				The desired count of solutions. </param>
		///
		///<returns>	True if we need to keep searching, false if there's no reason for more searching. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		static private bool FSearchForMultipleSolutions(
			TPs ps, ExpertSystem<TPs> es,
			BacktrackInfo bti,
			ICollection<TPs> lstpsSolutions,
			int csln)
		{
			// Is an expert system available?
			if (es != null)
			{
				// Apply rules and gather reasons.  Is it an impossible board?
				if (TryExpertSystem(es, ps, bti))
				{
					// Return true telling our caller to try something else
					return true;
				}
			}

			// Did we solve it with the expert system?
			if (ps.FSolved())
			{
				// Record the solution
				return RecordSolution(ps, bti, lstpsSolutions, csln);
			}

			// Didn't solve it by rules so start backtracking search

			// Get the list of potential moves from this partial board
			var lstIExtensions = ps.GetIExtensions();

			// Are there no moves from the current position?
			if (lstIExtensions.Count == 0)
			{
				// Are we keeping backtrack reasons?
				if (bti != null)
				{
					// Add that we've reached a leaf node
					bti.AddBacktrackReason(BacktrackReason.LeafNode);
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
				BacktrackInfo btiCur = null;

				// Are we keeping track of backtrack reasons?
				if (bti != null)
				{
					btiCur = new BacktrackInfo(btr, ext);

					// Add that we're trying this new move
					bti.AddBacktrackReason(btiCur);
				}

				// Apply the current extension
				var psCur = (TPs)ps.PsApply(ext, true);

				// Recursively search for a solution with this move
				var fContinueSearch = FSearchForMultipleSolutions(psCur, es, btiCur, lstpsSolutions, csln);

				// If there's no reason for more searching
				if (!fContinueSearch)
				{
					// Return false to tell our caller that
					return false;
				}
			}

			// Still haven't found our goal count - tell our caller to keep looking

			// If we're keeping backtrack reasons
			if (bti != null)
			{
				// Add that there are no more moves
				bti.AddBacktrackReason(BacktrackReason.NoMoreMoves);
			}
			return true;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Searches for a fixed count of solutions.  Returns as many up to that count as it can find. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="ps">				The puzzle/game up to this point. </param>
		/// <param name="cPlys">			Count of plys deep we've searched thus far</param>
		/// <param name="bti">				Backtrack info. </param>
		///
		///<returns>	An evaluation of how valuable the current board position is. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public static int EvaluateBoard(TPs ps, int cPlys, BacktrackInfo bti)
		{
			// Get the list of potential moves from this partial board
			var lstIExtensions = ps.GetIExtensions();

			// Are there no moves from the current position?
			if (lstIExtensions.Count == 0)
			{
				// Are we keeping backtrack reasons?
				if (bti != null)
				{
					// Add that we've reached a leaf node
					bti.AddBacktrackReason(BacktrackReason.LeafNode);
				}
				// Return the value of this leaf board
				return ps.Evaluate();
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

			var iVal = int.MinValue;

			// Try each extension in turn
			foreach (var ext in lstIExtensions)
			{
				BacktrackInfo btiCur = null;

				// Are we keeping track of backtrack reasons?
				if (bti != null)
				{
					btiCur = new BacktrackInfo(btr, ext);

					// Add that we're investigating this new move
					bti.AddBacktrackReason(btiCur);
				}

				// Apply the current extension
				var psCur = (TPs)ps.PsApply(ext, true);

				iVal = Math.Max(iVal, psCur.FContinueEvaluation(cPlys) ? EvaluateBoard(ps, cPlys + 1, btiCur) : ps.Evaluate());
			}

			return iVal;
		}

		private static bool RecordSolution(
			TPs ps,
			BacktrackInfo bti,
			ICollection<TPs> lstpsSolutions,
			int csln)
		{
			// Are we identical to solutions already identified?
			if (lstpsSolutions.Any(psCur => ps.IdenticalTo(psCur)))
			{
				// Are we keeping track of backtrack reasons?
				if (bti != null)
				{
					// Add Duplicate Solution as the reason for backtracking
					bti.AddBacktrackReason(BacktrackReason.DuplicateSolution);
				}
				return true;
			}

			// New solution - add to the list of solutions
			lstpsSolutions.Add((TPs)ps.Clone());

			// If we've found our goal count of solution, shut the whole operation down
			if (lstpsSolutions.Count == csln)
			{
				// Are we keeping track of backtrack reasons?
				if (bti != null)
				{
					// Add goal count reached reason
					bti.AddBacktrackReason(BacktrackReason.GoalCountReached);
				}

				// Return false to stop searching for other solutions
				return false;
			}

			// Are we keeping track of backtrack reasons?
			if (bti != null)
			{
				// Add that we found a soln but we need more
				bti.AddBacktrackReason(BacktrackReason.GoalReachedButMoreNeeded);
			}

			// Return true to keep looking for more solutions
			return true;
		}

		private static bool TryExpertSystem(ExpertSystem<TPs> es, TPs ps, BacktrackInfo bti)
		{
			List<ReasonRulePair> lstrrp;
			var fImpossible = es.FApply(ps, out lstrrp);

			// Are we keeping backtrack reasons?
			if (bti != null)
			{
				// Add that we applied the expert system instead of backtracking
				bti.AddExpertSystemReasons(lstrrp);
			}

			// Did we reach an impossible situation?
			if (!fImpossible)
			{
				// Are we keeping backtracing reasons?
				if (bti != null)
				{
					// Add that we detected an impossible board
					bti.AddBacktrackReason(BacktrackReason.RuleDetectedImpossibility);
				}
				// No possible solutions on this branch - Search other branches
				return true;
			}
			return false;
		}
	}
}
