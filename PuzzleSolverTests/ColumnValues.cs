using System;

namespace PuzzleSolverTests
{
	public struct ColumnValues
	{
		public byte Carry;
		public byte Add1;
		public byte Add2;
		public byte Sum;

		public ColumnValues(byte carry, byte add1, byte add2, byte sum)
		{
			Carry = carry;
			Add1 = add1;
			Add2 = add2;
			Sum = sum;
		}

		internal byte this[int iMember]
		{
			get
			{
				var member = (Member)iMember;
				switch (member)
				{
					case Member.Carry:
						return Carry;

					case Member.Add1:
						return Add1;

					case Member.Add2:
						return Add2;

					case Member.Sum:
						return Sum;
				}
				throw new InvalidOperationException("Bad value in ColumnValues indexer");
			}
		}
	}
}
