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
		bool Applied { get; set; }
		bool Impossible { get; set; }
	}

	public class ReasonBase : IReason
	{
		public bool Applied { get; set; }
		public bool Impossible { get; set; }
	}
}
