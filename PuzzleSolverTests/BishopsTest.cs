using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuzzleSolver;

namespace PuzzleSolverTests
{
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
	/// This is based on the "Little Bishops" problem in "Programming Challenges".  It's
	/// not efficient enough to count the 5599888 solutions in their example data.  I'm
	/// pretty sure that this is because it's not taking symmetry considerations into
	/// account as discussed below.
	/// 
	/// We use two longs as bitmaps to represent which squares are occupied and which
	/// are attacked.  Position 0-7 represents the first row from from left at bit 0 to right
	/// at bit 7.  The nextrow is positions 8-15, etc..  This allows only boards up to size 8
	/// but that's fine.
	/// 
	/// We could help ourselves a lot by symmetry conditions which we currently totally ignore.
	/// For instance, we could insist that the first bishop be placed on the upper left diagonal
	/// and the second be placed in the upper right diagonal.  There will certainly be exactly one
	/// bishop on both of these diagonals and all other solutions can be obtained by flipping 
	/// around one or both so each of our solutions would correspond to four symmetric solutions.
	/// Might have to be some fiddling with odd sized boards and fitting one into the center
	/// square so it's on both diagonals, but not a big deal.  We don't do this here and so
	/// full 8x8 boards are actually pretty impractical with the setup as it stands.
	/// 
	/// It wouldn't be that hard to incorporate this sort of idea.  GetIExtensions would have
	/// to look at the current number of bishops placed and restrict the positions for the
	/// first two bishops.
	/// 
	/// Another inefficiency is that we check each new solution to see if it's already been
	/// generated before.  With 5 million solutions to check against for each new one
	/// discovered, this is going to be a huge chunk of time.  We probably need to incorporate
	/// some sort of facility to indicate that each solution generated will be unique and
	/// there's no need to check it against the previous solutions.  We would need to ensure this
	/// though and the current desing doesn't ensure it.  Even if we did the symmetry restrictions
	/// mentioned above, the third bishop could be placed anywhere and ditto the fourth so their
	/// positions would be swapped in different solution paths which would end up with identical
	/// solutions.  We would have to be a little more ingenious in general to get this thing to
	/// work quickly.
	/// 
	/// I did a little of that ingenuity with the longs representing entire boards and keeping a
	/// list of attacked positions for each move, but too much of it would ruin the pedagogical
	/// nature of this example so I'll avoid them here.
	/// 
	/// The actual way to do this is to divide the board into black squares and white squares.
	/// for each of these boards, rotate them 45 degrees and you've got a rook placement position.
	/// Actually, you only need to solve one of these rook placement problems since they're
	/// independent and the total solutions is just the solutions for one squared.
	/// 
	/// There's a great mathematical solution here:
	/// http://blog.csdn.net/liukaipeng/article/details/3901412
	/// Goes to show that if you blindly start backtracking without even imagining that there might
	/// be a simpler solution - well, there might be.
	/// 
	/// </summary>
	/// <seealso cref="PuzzleSolver.IPartialSolution" />
	class PartialSolutionBishop : IPartialSolution
	{
		ulong Occupied { get; set; }
		ulong Attacked { get; set; }
		int BoardSize { get; set; }
		int BishopCount { get; set; }
		int BishopsRequired { get; set; }

		private static Dictionary<ulong, ulong> _attacksFromPositions;
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

		private ulong AttackedPositionsFromMask(ulong mask)
		{
			ulong attacks = 0;
			var iRow = _rowFromMask[mask];
			var iCol = _colFromMask[mask];
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

		public bool IdenticalTo(IPartialSolution ps)
		{
			return ((PartialSolutionBishop)ps).Occupied == Occupied;
		}

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

		public IPartialSolution PsApply(IExtension ext, bool fReturnClone)
		{
			var psb = fReturnClone ? new PartialSolutionBishop(this) : this;
			var mask = ((BishopExtension)ext).Position;
			psb.Occupied |= mask;
			psb.Attacked |= AttackedPositionsFromMask(mask);
			psb.BishopCount = BishopCount + 1;
			return psb;
		}

		public bool FSolved()
		{
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
