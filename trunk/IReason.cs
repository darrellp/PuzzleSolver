using System.Collections.Generic;

namespace PuzzleSolver
{
	public interface IReason
	{
		bool HasExtension();
		List<IExtension> GetExtensions();
	}
}
