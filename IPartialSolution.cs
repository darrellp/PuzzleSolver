using System.Collections.Generic;

namespace PuzzleSolver
{
	/// <summary>
	/// A partial solution is just what it sounds like - a partial solution to the problem at hand.  In
	/// terms of a search tree, it represents a node in the search tree.
	/// </summary>
	public interface IPartialSolution
	{
		// Is this solution identical to another one?
		bool IdenticalTo(IPartialSolution ps);

		// Get the potential extensions to this partial solution.  In terms of the search tree, these
		// are representatives of the children of this node.  If the count of the returned list is zero
		// then this is a leaf node.
		List<IExtension> GetIExtensions();

		// Apply an extension to get a new child node
		IPartialSolution PsApply(IExtension ext, bool fReturnClone);

		// Sometimes between the time we generate an extension and the time we try to apply it, information
		// may arise to make that extension obsolete.  If so, ExtensionFailed can return false.
		void ExtensionFailed(IExtension ext);

		// If this represents a solution, say so!
		bool FSolved();

		// Make a copy of this solution
		IPartialSolution Clone();
	}
}
