using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolver;

namespace PuzzleSolverTests
{
	internal enum Member
	{
		Carry,
		Add1,
		Add2,
		Sum,
		MemberCount
	};
	
	/// <summary>
	/// This structure represents a partially (or fully) solved alphametic/board.
	/// </summary>
	/// 
	/// <remarks>
	/// All PuzzleSolver applications must supply a structure representing a partially solved board.This is the data
	/// structure we backtrack with and use to produce child boards from in the backtracking process.
	/// 
	/// Generally, they should set rules which apply themselves to partial solutions and either refine the partial solution,
	/// do nothing at all or decide that the partial solution cannot lead to a solution.  This is not required but can greatly
	/// speed things along.  Without any such rules, the PuzzleSolver essentially just does blind backtracking.
	/// 
	/// They also have to decide which new partial solutions should be tested from a given partial solution.  This is what
	/// forms the backtracking tree.
	/// 
	/// For efficiency's sake, they also need to be able to tell when two partial solutions are identical so that we don't
	/// follow down the same solution tree twice.
	/// 
	/// The partial solution is also allowed to cut off the search process based on the plys deep we've searched.
	/// 
	/// Finally, it can provide a ranking to a partial solution to indicate which ones in a pool should be pursued first.
	/// 
	/// Finally, they include a way to determine that a partial solution is in fact fully solved.
	/// 
	/// In this case we represent the puzzle with the original strings that made the addends and the sum.  We also have a
	/// mapping from characters to values for characters that have taken on a fixed value.  For characters that haven't 
	/// taken on a fixed value, they are represented by a dictionary to Possibles, a data structure to represent the
	/// possible values a character could take on.  Finally, we have a list of carries for each column.
	/// </remarks>
	/// <seealso cref="PuzzleSolver.IPartialSolution" />
	class PartialSolutionAlphametic : IPartialSolution
	{
		public string Add1 { get; private set; }
		public string Add2 { get; private set; }
		public string Sum { get; private set; }
		public Dictionary<char, byte> Mapping { get; private set; }
		public Dictionary<char, Possibles> Possible { get; private set; }
		public byte[] Carries { get; private set; }

		internal const int Base = 10;
		internal const byte NoValue = Base;

		/// <summary>
		/// Gets or sets an assigned value for a given character
		/// </summary>
		/// <remarks>
		/// partialSolution['A'] gives the value A has taken on in this solution or NoValue if it hasn't been assigned
		/// a value yet.
		/// </remarks>
		/// <value>
		/// The value the given character has or should have.
		/// </value>
		/// <param name="key">The character.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">Setting previously set digit to a different value</exception>
		protected internal byte this[char key]
		{
			get { return Mapping[key]; }
			set
			{
				if (Mapping[key] == value)
				{
					return;
				}
				if (Mapping[key] != NoValue)
				{
					throw new InvalidOperationException("Setting previously set digit to a different value");
				}
				Mapping[key] = value;

				// The possibilities for this key is just the fixed value
				Possible[key] = new Possibles(1 << value);

				// No other key can have this value
				foreach (var ch in Mapping.Keys.Where(ch => ch != key))
				{
					Possible[ch].Disallow(value);
				}
			}
		}

		/// <summary>
		/// Gets all the values in a given column.
		/// </summary>
		/// <param name="iColumn">The index of the column.</param>
		/// <returns>A ColumnValues object with all the values</returns>
		internal ColumnValues GetColumnValues(int iColumn)
		{
			return new ColumnValues(
				ValueAt(Member.Carry, iColumn),
				ValueAt(Member.Add1, iColumn),
				ValueAt(Member.Add2, iColumn),
				ValueAt(Member.Sum, iColumn));
		}

		/// <summary>
		/// Gets a particular value at a particular column: ipos == 0 means the ones column
		/// </summary>
		/// <param name="member">The type value we're looking for.</param>
		/// <param name="ipos">The column index we're looking in.</param>
		/// <returns>The value or NoValue if none has been assigned</returns>
		internal byte ValueAt(Member member, int ipos)
		{
			var memberString = string.Empty;
			switch (member)
			{
				case Member.Add1:
					memberString = Add1;
					break;
				case Member.Add2:
					memberString = Add2;
					break;
				case Member.Sum:
					memberString = Sum;
					break;
				case Member.Carry:
					return ipos >= Carries.Length ? (byte)0 : Carries[ipos];
			}
			if (ipos >= memberString.Length)
			{
				return 0;
			}
			return Mapping[memberString[memberString.Length - 1 - ipos]];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PartialSolutionAlphametic"/> class.
		/// </summary>
		/// <param name="add1">The string for the upper addend.</param>
		/// <param name="add2">The string for the lower addend.</param>
		/// <param name="sum">The string for the sum.</param>
		internal PartialSolutionAlphametic(string add1, string add2, string sum)
		{
			var chars = (add1 + add2 + sum).Distinct().ToArray();
			Add1 = add1;
			Add2 = add2;
			Sum = sum;
			Mapping = chars.ToDictionary(c => c, c => NoValue);
			Possible = chars.ToDictionary(c => c, c => new Possibles(c));
			Carries = Enumerable.Repeat(NoValue, Sum.Length).ToArray();
			// No carry into ones digit
			Carries[0] = 0;
		}

		/// <summary>
		/// Clones the passed in partial solution.
		/// </summary>
		/// <param name="psa">The partial solution to be cloned.</param>
		internal PartialSolutionAlphametic(PartialSolutionAlphametic psa)
		{
			Mapping = new Dictionary<char, byte>(psa.Mapping);
			Carries = (byte[])psa.Carries.Clone();
			Possible = psa.Possible.ToDictionary(a => a.Key, a => new Possibles(a.Value));
			Add1 = psa.Add1;
			Add2 = psa.Add2;
			Sum = psa.Sum;
		}

		/// <summary>
		/// Sets the character c to not have a particular value.
		/// </summary>
		/// <param name="c">The character to be restricted.</param>
		/// <param name="val">The value it can't take on.</param>
		/// <returns>True if this was possible, false if this has removed the last possibility for c</returns>
		public bool SetImpossible(char c, byte val)
		{
			Possible[c].Disallow(val);
			if (Possible[c].Impossible)
			{
				return false;
			}
			if (Possible[c].Fixed)
			{
				this[c] = Possible[c].LowestValue();
			}
			return true;
		}

		/// <summary>
		/// Determine if this solution identical to another one.
		/// </summary>
		/// <param name="ps">The partial solution to be compared to this one.</param>
		/// <returns>
		/// true if they are identical.
		/// </returns>
		public bool IdenticalTo(IPartialSolution ps)
		{
			var psA = ps as PartialSolutionAlphametic;
			return psA != null && Possible.All(assoc => psA.Possible[assoc.Key].Values == assoc.Value.Values);
		}

		/// <summary>
		/// Get the potential extensions to this partial solution.  In terms of the search tree, these
		/// are representatives of the children of this node.  If the count of the returned list is zero
		/// then this is a leaf node.
		/// 
		/// In our particular case (alphametics) we find the first unfixed character and give all it's potential
		/// values as extensions.
		/// </summary>
		/// <returns>
		/// A list of applicable extensions.
		/// </returns>
		public List<IExtension> GetIExtensions()
		{
			foreach (var assoc in Mapping)
			{
				if (assoc.Value == NoValue)
				{
					var extensions = new List<IExtension>();
					for (byte i = 0; i < Base; i++)
					{
						if (Possible[assoc.Key].IsPossible(i))
						{
							extensions.Add(new ExtensionAlphametic(assoc.Key, i));
						}
					}
					return extensions;
				}
			}
			return null;
		}

		/// <summary>
		/// Apply an extension to get a new child node.
		/// </summary>
		/// <param name="ext">The extension to be applied.</param>
		/// <param name="fReturnClone">If true, the returned partial solution will be a newly created
		/// clone of this partial solution with the extension applied.  If
		/// false, we simply apply the extension to this partial solution and
		/// return "this".</param>
		/// <returns>
		/// A partial solution with the extension applied.
		/// </returns>
		public IPartialSolution PsApply(IExtension ext, bool fReturnClone)
		{
			var extAl = ext as ExtensionAlphametic;
			if (extAl == null)
			{
				return null;
			}
			var ret = new PartialSolutionAlphametic(this);
			ret[extAl.Char] = extAl.Value;
			return ret;
		}

		/// <summary>
		/// If this represents a solution, say so!
		/// </summary>
		/// <returns>
		/// true if this partial solution is a solution to the problem at hand.
		/// </returns>
		public bool FSolved()
		{
			return Mapping.All(assoc => assoc.Value != NoValue);
		}

		/// <summary>
		/// Makes a deep copy of this object.
		/// </summary>
		/// <returns>
		/// A copy of this object.
		/// </returns>
		public IPartialSolution Clone()
		{
			return new PartialSolutionAlphametic(this);
		}

		/// <summary>
		/// Continue evaluation at this node.
		/// </summary>
		/// <param name="cPlysDeep">The number of plys deep we've searched already.</param>
		/// <returns>
		/// true if this node should be expanded further.
		/// </returns>
		public bool FContinueEvaluation(int cPlysDeep)
		{
			return !FSolved();
		}

		/// <summary>
		/// Evaluates the board without any lookahead.
		/// </summary>
		/// <returns>
		/// Evaluation of the board.
		/// </returns>
		public int Evaluate()
		{
			// We don't really have any ranking between potential solutions.  We could perhaps rank
			// based on the number of characters which have taken on values or on the total number
			// of possible values across all characters.  I'm not sure how much good that would do.
			// For these demonstration purposes, we leave off this purely heuristic measure entirely.
			return 0;
		}
	}
}
