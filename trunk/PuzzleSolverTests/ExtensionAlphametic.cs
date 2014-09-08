using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleSolver;

namespace PuzzleSolverTests
{
	internal class ExtensionAlphametic : IExtension
	{
		public char Char { get; private set; }
		public byte Value { get; private set; }

		public ExtensionAlphametic(char character, byte value)
		{
			Char = character;
			Value = value;
		}

		public int CompareTo(object obj)
		{
			return 0;
		}
	}
}
