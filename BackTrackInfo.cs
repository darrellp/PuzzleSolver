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
	/// <summary>	Information about the backtracking process. This is intended to be a full
	/// accounting of the reasoning process when deriving a solution including all the reasoning of
	/// the expert system and all the decisions made during the backtracking process itself.</summary>
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

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets or sets the extension/move made. This only applies if the BackTrackReason 
		/// is ForcedChoice or Guess</summary>
		///
		/// <value>	The extension. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public IExtension Extension { get; private set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Constructor. </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="btReason">	The reason for the backtracking step. </param>
		/// <param name="lstrrp">	The rule/reason pairs during expert system application. </param>
		/// <param name="ext">		The extension/move made on this backtracking branch. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public BackTrackInfo(BacktrackReason btReason, List<ReasonRulePair> lstrrp, IExtension ext)
		{
			ReasonList = lstrrp;
			Extension = ext;
			BackTrackReason = btReason;
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	
		/// Returns a <see cref="T:System.String" /> that represents the current backtracking reason. 
		/// </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <returns>	
		/// A <see cref="T:System.String" /> that represents the current backtracking reason. 
		/// </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			return BackTrackReason + (Extension == null ? "" : Extension.ToString());
		}
	}
}