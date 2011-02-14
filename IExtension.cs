using System;

namespace PuzzleSolver
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// <para>Extensions are "representatives" of child nodes.  Partial solutions are normally somewhat expensive to 
	/// create and take up a lot of memory so we use extensions until it's time to actually attempt to solve a
	/// particular partial solution.  So for Battleship, an extension would normally just specify a ship size
	/// and a place to put it.  Applying that extension to a partial solution will actually produce a new 
	/// partial solution identical to the old one but with the specified ship placed in the specified spot.</para>
	///
	/// <para>IExtension supports IComparable so that we can potentially heuristically sort the child nodes in order
	/// of their likelihood of extending our solution.  Again, using the Battleship example, we would normally
	/// try placing ships in columns/rows where they fit most snugly, i.e., where the number of open cells is
	/// a minimum.</para>
	///
	/// <para>Note that IExtensions are used in a few places - backtracker, reasons and generators for instance.  There
	/// is nothing that says the same IExtension must be used for each of these.  For instance, in Battleship
	/// I use entire ships as the extension in Backtracker but the generator should use a smaller unit of
	/// extension - we don't want to show only entire ships in the generated puzzles so we would use a
	/// smaller extension (i.e., what goes in a single cell) for generation purposes.  On the other hand, in
	/// Sudoku, we extend and generate puzzles in the same units - the value that is assigned to one cell.
	/// The only requirement is that the Apply method of IPartialSolution knows how to deal with any extension
	/// we throw at it.</para>
	/// </summary>
	///
	/// <remarks>	Darrellp, 2/14/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public interface IExtension : IComparable
	{
	}
}
