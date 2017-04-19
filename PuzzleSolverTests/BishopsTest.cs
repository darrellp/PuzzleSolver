using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuzzleSolver;

namespace PuzzleSolverTests
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	(Unit Test Class) the bishops test. </summary>
	///
	/// <remarks>	This represents pretty much the simplest case where there are no rules
	/// 			at all - pure backtracking at work.
	/// 			Darrell Plank, 4/18/2017. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	[TestClass]
	public class BishopsTest
	{
		[TestMethod]
		public void TestBishopPlacement()
		{
			var psb = new PartialSolutionBishop(4,4);
			var psbSolutions = new List<PartialSolutionBishop>();
			Assert.IsTrue(Backtracker<PartialSolutionBishop>.FSearchForMultipleSolutions(
				psb, null, null, psbSolutions, int.MaxValue));
			Assert.AreEqual(260, psbSolutions.Count);
		}
	}

	/// <summary>
	/// This is based on the "Little Bishops" problem in 
	/// <a href="http://www.programming-challenges.com/pg.php?page=downloadproblem&format=html&probid=110801">
	/// Programming Challenges
	/// </a>.  
	/// It's not efficient enough to count the 5599888 solutions in their example data.  An efficient
	/// method is given at the end of these remarks.  This solution is just for learning purposes.
	/// 
	/// We use two longs as bitmaps to represent which squares are occupied and which
	/// are attacked.  Position 0-7 represents the first row from from left at bit 0 to right
	/// at bit 7.  The nextrow is positions 8-15, etc..  This allows only boards up to size 8
	/// but that's fine.
	/// 
	/// 
	/// The actual way to do this is to divide the board into black squares and white squares.
	/// for each of these boards, rotate them 45 degrees and you've got a rook placement position.
	/// Actually, you only need to solve one of these rook placement problems since they're
	/// independent and the total solutions is just the solutions for one squared.  Well, this
	/// might not work if n is odd since the "white" board and the "black" board would be
	/// different.
	/// 
	/// There's a great mathematical solution here:
	/// <a href="http://blog.csdn.net/liukaipeng/article/details/3901412">Little Bishop's Solution</a>
	/// Goes to show that if you blindly start backtracking without even imagining that there might
	/// be a simpler solution - well, there might be.
	/// 
	/// </summary>
	/// <seealso cref="PuzzleSolver.IPartialSolution" />
	class PartialSolutionBishop : IPartialSolution
	{
		ulong Occupied { get; set; }
		ulong Attacked { get; set; }
		int BoardSize { get; }
		int BishopCount { get; set; }
		int BishopsRequired { get; }

		/// <summary>	A mapping from positions to the positions they attack. </summary>
		private static Dictionary<ulong, ulong> _attacksFromPositions;

		/// <summary>	Two dictionaries to map from the mask for a position to the
		/// 			row and column for that position. Yes, it's an easy
		/// 			calculation but why do it over and over?
		/// </summary>
		private static Dictionary<ulong, int> _rowFromMask;
		private static Dictionary<ulong, int> _colFromMask; 

		public PartialSolutionBishop(int boardSize, int bishopsRequired)
		{
			if (boardSize > 8)
			{
				throw new ArgumentException("Trying to set board with size > 8");
			}
			BoardSize = boardSize;
			BishopCount = 0;
			BishopsRequired = bishopsRequired;
			Initialize();
		}

		public PartialSolutionBishop(PartialSolutionBishop psb)
		{
			BoardSize = psb.BoardSize;
			Occupied = psb.Occupied;
			Attacked = psb.Attacked;
			BishopCount = psb.BishopCount;
			BishopsRequired = psb.BishopsRequired;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Initializes this object. </summary>
		///
		/// <remarks>	Darrell Plank, 4/17/2017. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		private void Initialize()
		{
			if (_attacksFromPositions == null)
			{
				int iPos;
				ulong mask;

				_rowFromMask = new Dictionary<ulong, int>(64);
				_colFromMask = new Dictionary<ulong, int>(64);
				for (iPos = 0, mask = 1; iPos < 64; iPos++, mask <<= 1)
				{
					// Row and column are just a division and remainder by 8.
					_rowFromMask[mask] = iPos >> 3;
					_colFromMask[mask] = iPos & 7;
				}
				_attacksFromPositions = new Dictionary<ulong, ulong>(64);
				for (mask = 1; mask != 0; mask <<= 1)
				{
					_attacksFromPositions[mask] = AttackedPositionsFromMask(mask);
				}
			}

			// For smaller board sizes we want to disallow anything off the board
			for (var iRow = BoardSize; iRow < 8; iRow++)
			{
				for (var iCol = 0; iCol < 8; iCol++)
				{
					Attacked |= MaskFromPos(iRow, iCol);
				}
			}
			for (var iCol = BoardSize; iCol < 8; iCol++)
			{
				for (var iRow = 0; iRow < 8; iRow++)
				{
					Attacked |= MaskFromPos(iRow, iCol);
				}
			}
		}

		public ulong MaskFromPos(int iRow, int iCol)
		{
			return (ulong)1 << ((iRow << 3) + iCol);
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Make a mask of attacked positions from a position mask. </summary>
		///
		/// <remarks>	Darrell Plank, 4/17/2017. </remarks>
		///
		/// <param name="mask">	The position mask. </param>
		///
		/// <returns>	Mask with the attacked positions. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		private ulong AttackedPositionsFromMask(ulong mask)
		{
			ulong attacks = 0;
			var iRow = _rowFromMask[mask];
			var iCol = _colFromMask[mask];

			// For each row determine the potential columns for attacked positions and
			// 'or' in those positions to the list of attacked positions.
			for (var curRow = 0; curRow < 8; curRow++)
			{
				var curCol = curRow - iRow + iCol;
				if (curCol >= 0 && curCol <= 7)
				{
					attacks |= MaskFromPos(curRow, curCol);
				}
				curCol = iRow - curRow + iCol;
				if (curCol >= 0 && curCol <= 7)
				{
					attacks |= MaskFromPos(curRow, curCol);
				}
			}
			return attacks;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Determine if this solution identical to another one. </summary>
		///
		/// <remarks>	Darrell Plank, 4/17/2017. </remarks>
		///
		/// <param name="ps">	The partial solution to be compared to this one. </param>
		///
		/// <returns>	true if they are identical. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public bool IdenticalTo(IPartialSolution ps)
		{
			return ((PartialSolutionBishop)ps).Occupied == Occupied;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Get the potential extensions to this partial solution.  We're using the position masks for the
		/// bishop to represent extensions.
		/// </summary>
		///
		/// <remarks>	Darrell Plank, 4/17/2017. </remarks>
		///
		/// <returns>	A list of applicable extensions. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public List<IExtension> GetIExtensions()
		{
			var positions = new List<IExtension>();

			for (var iRow = 0; iRow < BoardSize; iRow++)
			{
				for (var iCol = 0; iCol < BoardSize; iCol++)
				{
					var mask = MaskFromPos(iRow, iCol);
					if ((mask & Attacked) == 0)
					{
						positions.Add(new BishopExtension(mask));
					}
				}
			}
			return positions;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Apply an extension to get a new child node. </summary>
		///
		/// <remarks>	In the case of the bishop's problem, an extension is essentially a position mask
		/// 			and "applying it" means placing a bishop at that position and marking all
		/// 			it's attacked positions appropriately.  Darrell Plank, 4/17/2017. </remarks>
		///
		/// <param name="ext">		   	The extension to be applied. </param>
		/// <param name="fReturnClone">	If true, the returned partial solution will be a newly created
		/// 							clone of this partial solution with the extension applied.  If
		/// 							false, we simply apply the extension to this partial solution and
		/// 							return "this". </param>
		///
		/// <returns>	A partial solution with the extension applied. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public IPartialSolution PsApply(IExtension ext, bool fReturnClone)
		{
			var psb = fReturnClone ? new PartialSolutionBishop(this) : this;
			var mask = ((BishopExtension)ext).Position;
			
			// Place the new bishop
			psb.Occupied |= mask;

			// Mark all it's attacked positions
			psb.Attacked |= AttackedPositionsFromMask(mask);

			// Count it in our total
			psb.BishopCount = BishopCount + 1;
			return psb;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	If this represents a solution, say so! </summary>
		///
		/// <remarks>	Darrell Plank, 4/17/2017. </remarks>
		///
		/// <returns>	true if this board has been solved. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public bool FSolved()
		{
			// It's solved if we have the requested number of bishops
			return BishopCount == BishopsRequired;
		}

		public IPartialSolution Clone()
		{
			return new PartialSolutionBishop(this);
		}

		public bool FContinueEvaluation(int cPlysDeep)
		{
			return true;
		}

		public int Evaluate()
		{
			return 0;
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Just a position to place a new piece on the board. </summary>
	///
	/// <remarks>	Darrell Plank, 4/18/2017. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	class BishopExtension : IExtension
	{
		public ulong Position { get; set; }
		public int CompareTo(object obj)
		{
			return 0;
		}

		public BishopExtension(ulong position)
		{
			Position = position;
		}
	}
}
