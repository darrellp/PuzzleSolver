using System.Collections.Generic;

namespace PuzzleSolver
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	
	/// Rules act on partial solutions.  When we attempt to apply a rule to a partial solution, one
	/// of three things happen:
	/// 
	/// * The rule makes changes to the partial solution. Normally, rules try to deduce information
	/// about the partial solution from it's current state.  If they detect such a condition, they
	/// make changes to the state to indicate those changes.  For instance, a Sudoku puzzle may
	/// deduce that a particular digit goes in a particular cell and place it there.  FApply will
	/// return true if such a change was made.
	/// 
	/// * The rule determines that the current partial solution is unsolveable. Since rules often do
	/// fairly extensive analysis on the partial solutions, they will often be in a position to
	/// determine that the partial solution is unsolvable. For instance, if Sudoku determines, via an
	/// elimination rule, that there's only one value left in a row, it can deduce what goes there by
	/// the process of elimination. However, if that value is already in the crossing column, then
	/// the partial solution is unsolvable.  If the rule detects unsolvability, it will set
	/// fImpossible and return.
	/// 
	/// * The rule finds nothing of interest. If the rule simply turns up nothing, then it will return
	/// false and make no changes to the board. 
	/// </summary>
	///
	/// <remarks>	Darrellp, 2/14/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	// NOTE: We ought to change this to be dependent on the PS type...
	public interface IRule
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Returns true if the rule should be triggered. Used for a quick check before actually
		/// attempting to apply the rule. 
		/// </summary>
		///
		/// <param name="ps">	The partial solution. </param>
		///
		/// <returns>	true if the rule is applicable to the partial solution. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		bool FTrigger(IPartialSolution ps);

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Returns false if the rule had no effect - also tell if an the partial solution was found to
		/// be impossible. 
		/// </summary>
		///
		/// <param name="obj">			The object to apply the rule to. </param>
		/// <param name="reason">		[out] The reason the rule applied. </param>
		/// <param name="fImpossible">	[out] true if the board was determined to be impossible to solve. </param>
		///
		/// <returns>	true if the rule caused a change to the board, false if it made no changes. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		bool FApply(IPartialSolution obj, out List<IReason> reason, out bool fImpossible);
	}
}
