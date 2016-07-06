using System.Collections.Generic;

namespace PuzzleSolver
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	
	/// A partial solution is just what it sounds like - a partial solution to the problem at hand.
	/// </summary>
	///
	/// <remarks>	
	/// This is really the key structure to PuzzleSolver.  It contains some required methods and some
	/// which are heuristic but can greatly enhance performance.
	/// 
	/// In terms of a search tree, it represents a node in the search tree. This is the data structure we 
	/// backtrack with and use to produce child boards from in the backtracking process.
	/// 
	/// Generally, partialSolutions should provide rules which apply themselves to partial solutions and either refine the partial 
	/// solution, do nothing at all or decide that the partial solution cannot lead to a solution.  This is not required but can greatly
	/// speed things along.  Without any such rules, the PuzzleSolver essentially just does blind backtracking.
	/// 
	/// Rules are one of the key components of PuzzleSolver.  It's assumed that rules are more efficient than blind
	/// backtracking so the general procedure is to apply rules until none of them apply and then generate children from the
	/// final board which we backtrack down into.
	/// 
	/// They also have to decide which new partial solutions should be tested from a given partial solution.  This is what
	/// forms the backtracking tree.
	/// 
	/// For efficiency's sake, they also need to be able to tell when two partial solutions are identical so that we don't
	/// follow down the same solution tree twice.
	/// 
	/// The partial solution is also allowed to cut off the search process based on the plys deep we've searched.
	/// 
	/// It can provide a ranking to a partial solution to indicate which ones in a pool should be pursued first.
	/// 
	/// Finally, they include a way to determine that a partial solution is in fact a full solution.
	/// 
	/// Darrellp, 2/14/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public interface IPartialSolution
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Determine if this solution identical to another one. </summary>
		///
		/// <param name="ps">	The partial solution to be compared to this one. </param>
		///
		/// <returns>	true if they are identical. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		bool IdenticalTo(IPartialSolution ps);

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Get the potential extensions to this partial solution.  In terms of the search tree, these
		/// are representatives of the children of this node.  If the count of the returned list is zero
		/// then this is a leaf node. 
		/// </summary>
		///
		/// <returns>	A list of applicable extensions. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		List<IExtension> GetIExtensions();

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Apply an extension to get a new child node. </summary>
		///
		/// <param name="ext">			The extension to be applied. </param>
		/// <param name="fReturnClone">	If true, the returned partial solution will be a newly created
		/// 							clone of this partial solution with the extension applied.  If
		/// 							false, we simply apply the extension to this partial solution and
		/// 							return "this". </param>
		///
		/// <returns>	A partial solution with the extension applied. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		IPartialSolution PsApply(IExtension ext, bool fReturnClone);

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	If this represents a solution, say so! </summary>
		///
		/// <returns>	true if this partial solution is a solution to the problem at hand. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		bool FSolved();

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Makes a deep copy of this object. </summary>
		///
		/// <returns>	A copy of this object. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		IPartialSolution Clone();

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Continue evaluation at this node. </summary>
		///
		/// <param name="cPlysDeep">	The number of plys deep we've searched already. </param>
		///
		/// <returns>	true if this node should be expanded further. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		bool FContinueEvaluation(int cPlysDeep);

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Evaluates the board without any lookahead. </summary>
		///
		/// <returns>	Evaluation of the board. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		int Evaluate();
	}
}
