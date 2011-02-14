using System.Collections.Generic;

namespace PuzzleSolver
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Values that represent the reason for a backtrack . </summary>
	///
	/// <remarks>	Darrellp, 2/14/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public enum BacktrackReason
	{
		InitialEntry,
		ForcedChoice,
		Guess,
		ExpertSystemApplication,
		LeafNode,
		NoMoreMoves,
		RuleDetectedImpossibility,
		GoalReachedButMoreNeeded,
		GoalCountReached,
		DuplicateSolution
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Information about the backtracking process. </summary>
	///
	/// <remarks>	Darrellp, 2/14/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public class BackTrackInfo
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets the reason for the backtrack. </summary>
		///
		/// <value>	The back track reason. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public BacktrackReason BackTrackReason { get; private set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets the list of reasons. This only applies if the BackTrackReason 
		/// is ExpertSystemApplication</summary>
		///
		/// <value>	A List of reasons. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public List<ReasonRulePair> ReasonList { get; private set; }
		public IExtension Extension { get; private set; }

		public BackTrackInfo(BacktrackReason btReason, List<ReasonRulePair> lstrrp, IExtension ext)
		{
			ReasonList = lstrrp;
			Extension = ext;
			BackTrackReason = btReason;
		}

		public override string ToString()
		{
			return BackTrackReason + (Extension == null ? "" : Extension.ToString());
		}
	}
}