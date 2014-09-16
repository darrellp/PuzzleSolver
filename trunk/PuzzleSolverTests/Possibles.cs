using System;
using System.Text;

namespace PuzzleSolverTests
{
	/// <summary>
	/// Class for keeping track of the possibilities for a given value
	/// </summary>
	struct Possibles
	{
		public string Name;
		public int Values;
		public bool IsCarry;

		public bool Impossible
		{
			get { return Values == 0; }
		}

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

		public Possibles(Possibles possibles)
		{
			Name = possibles.Name;
			Values = possibles.Values;
			IsCarry = possibles.IsCarry;
		}

		public static Possibles Carry(int column, int values = 0x3)
		{
			return new Possibles("C" + column, values, true);
		}

		public void Disallow(int i)
		{
			if ((IsCarry && i > 1) || i > 9 || i < 0)
			{
				throw new ArgumentException("Invalid value in Disallow");
			}
			Values = Values & ~(1 << i);
		}

		public void Set(int p)
		{
			Values &= p;
		}

		public static int Add(int p1, int p2, bool subtract = false)
		{
			var add = p2;
			var sum = 0;
			// Take care of common case...
			if (p1 == 0x3ff || p2 == 0x3ff)
			{
				return 0x3ff;
			}
			for (var mask = subtract ? (1 << 10) : 1;
				subtract ? mask > 0 : mask < (1 << 10);
				mask = subtract ? (mask >> 1) : (mask << 1), add <<= 1)
			{
				if ((p1 & mask) != 0)
				{
					sum |= add;
				}
			}
			return 0x3ff & (sum | (sum >> 10));
		}

		public static int AddCarry(int n, int c, bool subtract)
		{
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
			if (IsCarry)
			{
				return AddCarry(possibles.Values, Values, subtract);
			}
			if (possibles.IsCarry)
			{
				return AddCarry(Values, possibles.Values, subtract);
			}
			return Add(Values, possibles.Values, subtract);
		}

		public int Add(int values, bool subtract = false)
		{
			if (IsCarry)
			{
				return AddCarry(values, Values, subtract);
			}
			return Add(Values, values, subtract);
		}

		public int Subtract(Possibles possibles)
		{
			return Add(possibles, true);
		}

		public int Subtract(int values)
		{
			return Add(values, true);
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
				"{" + sb.ToString() + "}";
		}
	}
}
