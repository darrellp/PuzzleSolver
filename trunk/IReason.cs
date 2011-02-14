using System.Collections.Generic;

namespace PuzzleSolver
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Interface representing the reason an expert system rule was applied </summary>
	///
	/// <remarks>	Darrellp, 2/14/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public interface IReason
	{
		bool HasExtension();
		List<IExtension> GetExtensions();
	}
}
