﻿using System.Collections.Generic;

namespace PuzzleSolver
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	
	/// A partial solution is just what it sounds like - a partial solution to the problem at hand.
	/// In terms of a search tree, it represents a node in the search tree. 
	/// </summary>
	///
	/// <remarks>	Darrellp, 2/14/2011. </remarks>
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
		/// <summary>	
		/// Sometimes between the time we generate an extension and the time we try to apply it,
		/// information may arise to make that extension obsolete.  If so, ExtensionFailed can return false. 
		/// </summary>
		///
		/// <param name="ext">	The extension to be applied. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		void ExtensionFailed(IExtension ext);

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
	}
}
