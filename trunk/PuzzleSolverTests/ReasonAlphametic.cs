using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver;

namespace PuzzleSolverTests
{
	class ReasonSum : ReasonBase
	{
		public ReasonSum(int column, byte carry, byte add1, byte add2, byte sum, byte newValue)
		{
			Column = column;
			Carry = carry;
			Add1 = add1;
			Add2 = add2;
			Sum = sum;
			NewValue = newValue;
		}

		public int Column { get; private set; }
		public byte Carry { get; private set; }
		public byte Add1 { get; private set; }
		public byte Add2 { get; private set; }
		public byte Sum { get; private set; }
		public byte NewValue { get; private set; }
	}
	class ImpossibleSum : ReasonBase
	{
		public ImpossibleSum(int column, byte carry, byte add1, byte add2, byte sum)
		{
			Column = column;
			Carry = carry;
			Add1 = add1;
			Add2 = add2;
			Sum = sum;
		}

		public int Column { get; private set; }
		public byte Carry { get; private set; }
		public byte Add1 { get; private set; }
		public byte Add2 { get; private set; }
		public byte Sum { get; private set; }
	}

	class GenerateCarry : ReasonBase
	{
		public GenerateCarry(int column, byte value)
		{
			Column = column;
			this.value = value;
		}

		public int Column { get; private set; }
		public byte value { get; private set; }
	}

	class ImpossibleCarry : ReasonBase
	{
		public ImpossibleCarry(int column, byte incompatibleValue)
		{
			Column = column;
			IncompatibleValue = incompatibleValue;
		}

		public int Column { get; private set; }
		public byte IncompatibleValue { get; private set; }
	}

	class DisableLeadingZero : ReasonBase
	{
		public DisableLeadingZero(char c)
		{
			Char = c;
		}

		public char Char { get; private set; }
	}

	class ImpossibleSizes : ReasonBase
	{
	}
}
