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
		/// <summary> Initial entry into the backtracking process.  </summary>
		InitialEntry,
		/// <summary> A forced move on the current board.  </summary>
		ForcedChoice,
		/// <summary> A  choice which may be backtracked over later.  </summary>
		Guess,
		/// <summary> An application of the expert system rules.  </summary>
		ExpertSystemApplication,
		/// <summary> A board from which there are no legal moves.  </summary>
		LeafNode,
		/// <summary> There are moves but we've tried them all without success.  </summary>
		NoMoreMoves,
		/// <summary> A board which has been detected to have no solution.  </summary>
		RuleDetectedImpossibility,
		/// <summary> Found a solution but we need more to satisfy our goal.  </summary>
		GoalReachedButMoreNeeded,
		/// <summary> Found all the solutions we need so backtracking is being shut down.  </summary>
		GoalCountReached,
		/// <summary> Found a goal but it's already been found before.  </summary>
		DuplicateSolution
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Information about the backtracking process. This is intended to be a full
	/// accounting of the reasoning process when deriving a solution including all the reasoning of
	/// the expert system and all the decisions made during the backtracking process itself.</summary>
	///
	/// <remarks>	Darrellp, 2/14/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public class BacktrackInfo
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
		/// <summary>	Gets the extension/move made. This only applies if the BackTrackReason 
		/// is ForcedChoice or Guess</summary>
		///
		/// <value>	The extension. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public IExtension Extension { get; private set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Gets the extensions which were made from this node. </summary>
		///
		/// <value>	The extensions. </value>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public List<BacktrackInfo> Extensions { get; private set; }

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Constructor. </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="btReason">	The reason for the backtracking step. </param>
		/// <param name="ext">		The extension/move made on this backtracking branch. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public BacktrackInfo(BacktrackReason btReason, IExtension ext)
		{
			ReasonList = new List<ReasonRulePair>();
			Extension = ext;
			BackTrackReason = btReason;
			Extensions = new List<BacktrackInfo>();
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

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Adds an expert system reason for a move made. </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="lstrrp">	The rules applied and the reason for the rule applications. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		internal void AddExpertSystemReasons(List<ReasonRulePair> lstrrp)
		{
			ReasonList.AddRange(lstrrp);
		}

		internal void AddBacktrackReason(BacktrackReason btr)
		{
			Extensions.Add(new BacktrackInfo(btr, null));
		}
		internal void AddBacktrackReason(BacktrackInfo bti)
		{
			Extensions.Add(bti);
		}
	}
}