using System;
using System.Collections.Generic;
using System.Text;

namespace PuzzleSolver
{
	public interface IReason
	{
		bool HasExtension();
		List<IExtension> GetExtensions();
	}
}
