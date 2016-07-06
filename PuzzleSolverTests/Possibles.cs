using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleSolverTests
{
    /// <summary>
    /// Class for keeping track of the possibilities for a given value in an alphametic.
    /// </summary>
    /// <remarks>
    /// These values include both the "letters" in the alphametic and also the "carries" which have no
    /// name and can only take values 0 and 1.  The possible values are kept in the bits of Values with 0
    /// corresponding to the 1's bit.
    /// 
    /// Should these be structs?  If they are then each time we want to change one we have to build a
    /// brand new one, return it and the caller has to replace it wherever it came from whereas if they're
    /// objects, they just change themselves in place.  I think the latter is better since generally we'll
    /// only allocate a few of them.  Then again, when cloning PSs they have to be reproduced.  Hmm...
    /// </remarks>
	class Possibles
	{
		public string Name { get; private set; }
		public int Values { get; private set; }
		public bool IsCarry { get; private set; }

        /// <summary>
        /// Gets a value indicating whether there are any possible values for this variable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if no values are available; otherwise, <c>false</c>.
        /// </value>
		public bool Impossible
		{
			get { return Values == 0; }
		}

        /// <summary>
        /// Gets a value indicating whether this <see cref="Possibles"/> has a single fixed value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if fixed; otherwise, <c>false</c>.
        /// </value>
		public bool Fixed
		{
			get { return !Impossible && (Values & (Values - 1)) == 0; }
		}

		public Possibles(int values = 0x3ff, bool isCarry = false)
		{
			IsCarry = isCarry;
			Values = values;
			Name = "?";
		}

		public Possibles(string name, int values = 0x3ff, bool isCarry = false)
		{
			IsCarry = isCarry;
			Values = values;
			Name = name;
		}

		public Possibles(char name, int values = 0x3ff, bool isCarry = false) : this(name.ToString(), values, isCarry) { }
		
		public Possibles(Possibles possibles)
		{
			Name = possibles.Name;
			Values = possibles.Values;
			IsCarry = possibles.IsCarry;
		}

        /// <summary>
        /// Determines whether the specified n is possible for this variable.
        /// </summary>
        /// <param name="n">The n being inquired about.</param>
        /// <returns>True if this variable can take on value n</returns>
		public bool IsPossible(int n)
		{
			return ((1 << n) & Values) != 0;
		}

        /// <summary>
        /// Produces a carry variable for a specific column.
        /// </summary>
        /// <remarks>
        /// Normally carry variables can only take on the values 0 and 1.  This is true for a single pair
        /// of values in a sum and is the default, though it can be overridden.
        /// </remarks>
        /// <param name="column">The column.</param>
        /// <param name="values">The possible values the carry can take on.  Defaults to 0 and 1.</param>
        /// <returns></returns>
		public static Possibles Carry(int column, int values = 0x3)
		{
			return new Possibles("C" + column, values, true);
		}

        /// <summary>
        /// Disallows the specified value.
        /// </summary>
        /// <param name="i">The value to be disallowed.</param>
        /// <exception cref="System.ArgumentException">Invalid value in Disallow</exception>
		public void Disallow(int i)
		{
			if ((IsCarry && i > 1) || i > 9 || i < 0)
			{
				throw new ArgumentException("Invalid value in Disallow");
			}
			Values &= ~(1 << i);
		}

        /// <summary>
        /// Lowest value this variable can take on.
        /// </summary>
        /// <returns>The lowest value</returns>
		public byte LowestValue()
		{
			var mask = 1;
			for (byte i = 0; i < 10; i++)
			{
				if ((Values & mask) != 1)
				{
					return i;
				}
				mask <<= 1;
			}
			return PartialSolutionAlphametic.NoValue;
		}


        /// <summary>
        /// Sets the possible values.
        /// </summary>
        /// <param name="p">The possible values in the bits of p.</param>
		public void Set(int p)
		{
			Values &= p;
		}

        /// <summary>
        /// Creates a bitmap for all the values in a list of values.
        /// </summary>
        /// <remarks>
        /// We don't do error checking.  If you want to set a value of 18, this function will not complain.
        /// </remarks>
        /// <param name="vals">The vals to be set.</param>
        /// <returns>The int with the proper bits set for these values</returns>
		static public int ToValuesInt(List<int> vals)
		{
			return vals.Aggregate(0, (current, val) => current | (1 << val));
		}

		public static int Add(int p1, int p2, bool subtract = false)
		{
			bool fCarries0, fCarries1;
			return Add(p1, p2, out fCarries0, out fCarries1, subtract);
		}

        /// <summary>
        /// Adds two possibles together and returns the possibilities for the sum and the two carries generated
        /// </summary>
        /// <param name="p1">The first possible.</param>
        /// <param name="p2">The second possible.</param>
        /// <param name="fCarries0">if set to <c>true</c> we definitely carry 0.</param>
        /// <param name="fCarries1">if set to <c>true</c> we definitely carry 1.</param>
        /// <param name="subtract">if set to <c>true</c> do a subtract rather than an add.</param>
        /// <returns></returns>
		public static int Add(int p1, int p2, out bool fCarries0, out bool fCarries1, bool subtract = false)
		{
			var add = p2;
			var sum = 0;
			fCarries0 = true;
			fCarries1 = true;

			// Take care of common case...
			if (p1 == 0x3ff || p2 == 0x3ff)
			{
                // Totally unknown plus totally unknown yields totally unknown outputs
				return 0x3ff;
			}
			for (var mask = subtract ? (1 << 10) : 1;
				subtract ? mask > 0 : mask < (1 << 10);
				mask = subtract ? (mask >> 1) : (mask << 1), add <<= 1)
			{
				if ((p1 & mask) == 0) continue;

				// If any of the sums are less than ten then we can't guarantee a carry of 1
				if ((add & 0x3ff) != 0)
				{
					fCarries1 = false;
				}
				// Likewise, if any of the sums are 10 or greater then we can't guarantee a carry of 0
				else if ((add & ~0x3ff) != 0)
				{
					fCarries0 = false;
				}
				sum |= add;
			}
			return 0x3ff & (sum | (sum >> 10));
		}

		public static int AddCarry(int n, int c, bool subtract)
		{
			bool fCarry0, fCarry1;
			return AddCarry(n, c, out fCarry0, out fCarry1, subtract);

		}

		public static int AddCarry(int n, int c, out bool fCarry0, out bool fCarry1, bool subtract)
		{
			if (subtract)
			{
				// We will absolutely carry 0 if either n > 0 or if the carry is fixed at 0
				fCarry0 = ((n & 1) == 0) || c == 1;
				// We will absolutely carry 1 only if n == 0 and c == 1
				fCarry1 = n == 0x1 && c == 0x2;
			}
			else
			{
				// We will absolutely carry 0 if either n < 9 or if we are fixed at 0
				fCarry0 = (n & 0x200) == 0 || c == 1;
				// We will absolutely carry 1 only if n == 9 and c == 1 (well, of course, the bit versions of these)
				fCarry1 = n == 0x200 && c == 0x2;
			}
			switch (c)
			{
				case 0:
					return 0;

				case 1:
					return n;

				case 2:
				case 3:
					var raw = subtract ? n >> 1 : n << 1;
					if ((raw & 0x400) != 0)
					{
						raw++;

					}
					else if (subtract && ((n & 1) != 0))
					{
						raw += 1 << 9;
					}
					return (c == 2 ? raw : (n | raw)) & 0x3ff;

				default:
					throw new ArgumentException("Bad value in AddCarryTo");

			}
		}

		public int Add(Possibles possibles, bool subtract = false)
		{
			bool fCarry0, fCarry1;
			return Add(possibles, out fCarry0, out fCarry1, subtract);
		}

		public int Add(Possibles possibles, out bool fCarries0, out bool fCarries1, bool subtract = false)
		{
			if (IsCarry)
			{
				return AddCarry(possibles.Values, Values, out fCarries0, out fCarries1, subtract);
			}
			if (possibles.IsCarry)
			{
				return AddCarry(Values, possibles.Values, out fCarries0, out fCarries1, subtract);
			}
			return Add(Values, possibles.Values, out fCarries0, out fCarries1, subtract);
		}

		public int Add(int values, bool subtract = false)
		{
			bool fCarry0, fCarry1;
			return Add(values, out fCarry0, out fCarry1, subtract);
		}

		public int Add(int values, out bool fCarries0, out bool fCarries1, bool subtract = false)
		{
			if (IsCarry)
			{
				return AddCarry(values, Values, out fCarries0, out fCarries1, subtract);
			}
			return Add(Values, values, out fCarries0, out fCarries1, subtract);
		}

		public int Subtract(Possibles possibles)
		{
			bool fCarry0, fCarry1;
			return Subtract(possibles, out fCarry0, out fCarry1);
		}

		public int Subtract(Possibles possibles, out bool fCarries0, out bool fCarries1)
		{
			return Add(possibles, out fCarries0, out fCarries1, true);
		}

		public int Subtract(int values)
		{
			bool fCarries0, fCarries1;
			return Subtract(values, out fCarries0, out fCarries1);
		}

		public int Subtract(int values, out bool fCarries0, out bool fCarries1)
		{
			return Add(values, out fCarries0, out fCarries1, true);
		}

		public override string ToString()
		{
			if (Impossible)
			{
				return Name + ":" + " Impossible";
			}
			return String.Format("{0} = {1}", Name, ValuesString(this));
		}

		private static object ValuesString(Possibles possibles)
		{
			var sb = new StringBuilder();
			var insertComma = false;
			var values = possibles.Values;
			for (var i = 0; i < 10; i++)
			{
				if ((values & 1) != 0)
				{
					sb.Append((insertComma ? "," : "") + i);
					insertComma = true;
				}
				values >>= 1;
			}
			return possibles.Fixed ?
				sb.ToString() :
				"{" + sb + "}";
		}
	}
}
