namespace PuzzleSolver
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	A structure to record a rule application and the reason for that application. </summary>
	///
	/// <remarks>	Darrellp, 2/14/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public struct ReasonRulePair
	{
		public IReason Reason;
		internal IRule Rule;

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Constructor. </summary>
		///
		/// <remarks>	Darrellp, 2/14/2011. </remarks>
		///
		/// <param name="reasonParm">	The reason the rule was applied. </param>
		/// <param name="ruleParm">		The rule that was applied. </param>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public ReasonRulePair(IReason reasonParm, IRule ruleParm)
		{
			Reason = reasonParm;
			Rule = ruleParm;
		}
	}
}