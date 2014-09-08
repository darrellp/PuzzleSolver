using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolver;

namespace PuzzleSolverTests
{
	internal enum Member
	{
		Add1,
		Add2,
		Sum,
		Carry,
		MemberCount
	};
	
	// Implemented as a dictionary mapping letters to digits.  Currently
	// unset letters are mapped to NoValue (10).
	class PartialSolutionAlphametic : IPartialSolution
	{
		public string Add1 { get; private set; }
		public string Add2 { get; private set; }
		public string Sum { get; private set; }
		public Dictionary<char, byte> Mapping { get; private set; }
		public Dictionary<char, bool[]> Possible { get; private set; }
		public byte[] Carries { get; private set; }
		public bool LeadingZeroesChecked { get; set; }

		internal const int Base = 10;
		internal const byte NoValue = Base;

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
				for (var i = 0; i < Base; i++)
				{
					Possible[key][i] = i == value;
				}
				foreach (var ch in Mapping.Keys.Where(ch => ch != key))
				{
					Possible[ch][value] = false;
				}
			}
		}

		// ipos == 0 means the ones column...
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

		internal PartialSolutionAlphametic(string add1, string add2, string sum)
		{
			var chars = (add1 + add2 + sum).Distinct().ToArray();
			Add1 = add1;
			Add2 = add2;
			Sum = sum;
			Mapping = chars.ToDictionary(c => c, c => NoValue);
			Possible = chars.ToDictionary(c => c, c => Enumerable.Repeat(true, Base).ToArray());
			Carries = Enumerable.Repeat(NoValue, Sum.Length).ToArray();
			// No carry into ones digit
			Carries[0] = 0;
		}

		internal PartialSolutionAlphametic(PartialSolutionAlphametic psa)
		{
			Mapping = new Dictionary<char, byte>(psa.Mapping);
			Carries = (byte[])psa.Carries.Clone();
			Possible = psa.Possible.ToDictionary(a => a.Key, a => (bool[]) a.Value.Clone());
			Add1 = psa.Add1;
			Add2 = psa.Add2;
			Sum = psa.Sum;
		}

		public bool SetImpossible(char c, byte val)
		{
			Possible[c][val] = false;
			var count = Possible[c].Count(f => f);
			if (count == 0)
			{
				return false;
			}
			if (count == 1)
			{
				var n = Possible[c].Select((f, index) => index).First();
				this[c] = (byte)n;
			}
			return true;
		}

		public bool IdenticalTo(IPartialSolution ps)
		{
			var psA = ps as PartialSolutionAlphametic;
			return psA != null && Possible.All(assoc => psA.Possible[assoc.Key].SequenceEqual(assoc.Value));
		}

		public List<IExtension> GetIExtensions()
		{
			foreach (var assoc in Mapping)
			{
				if (assoc.Value == NoValue)
				{
					var extensions = new List<IExtension>();
					for (byte i = 0; i < Base; i++)
					{
						if (Possible[assoc.Key][i])
						{
							extensions.Add(new ExtensionAlphametic(assoc.Key, i));
						}
					}
					return extensions;
				}
			}
			return null;
		}

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

		public bool FSolved()
		{
			return Mapping.All(assoc => assoc.Value != NoValue);
		}

		public IPartialSolution Clone()
		{
			return new PartialSolutionAlphametic(this);
		}

		public bool FContinueEvaluation(int cPlysDeep)
		{
			return !FSolved();
		}

		public int Evaluate()
		{
			return 0;
		}
	}
}
