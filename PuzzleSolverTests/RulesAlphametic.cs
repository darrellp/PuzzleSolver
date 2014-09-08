using System.Collections.Generic;
using PuzzleSolver;

namespace PuzzleSolverTests
{
	/// <summary>
	/// Rules for Alphametics
	/// </summary>
	/// 
	/// <remarks>
	/// These three rules essentially set the required rules for an alphametic - the leading digits can't be zero,
	/// each character gets a unique value and the sums have to check out. There are all sorts of other common sense
	/// rules we could put in - if we have two values in a column but no carry the sum is either the sum of the two
	/// values or that sum plus 1 for instance - but for simplicity's sake we're sticking with the basics here. 
	/// </remarks>
	class RulesAlphametic
	{
		internal static List<IRule> Rules
		{
			get
			{
				return new List<IRule>
				{
					new ApplyAdditionRule(),
				};
			}
		}

		internal static List<IRule> OneTimeRules
		{
			get
			{
				return new List<IRule>
				{
					new NoLeadingZeroRule(),
					new CheckLengthsRule(),
				};
			}
		}

		// One time rule that ensures that no leading digits are zero
		class NoLeadingZeroRule : IRule
		{
			public bool FTrigger(IPartialSolution ps)
			{
				return !((PartialSolutionAlphametic)ps).LeadingZeroesChecked;
			}

			public bool FApply(IPartialSolution obj, out List<IReason> reason, out bool fImpossible)
			{
				var psa = (PartialSolutionAlphametic)obj;
				psa.SetImpossible(psa.Add1[0], 0);
				psa.SetImpossible(psa.Add2[0], 0);
				psa.SetImpossible(psa.Sum[0], 0);
				fImpossible = false;
				reason = null;
				return true;
			}
		}

		class CheckLengthsRule : IRule
		{
			public bool FTrigger(IPartialSolution ps)
			{
				return !((PartialSolutionAlphametic)ps).LeadingZeroesChecked;
			}

			public bool FApply(IPartialSolution obj, out List<IReason> reason, out bool fImpossible)
			{
				var psa = (PartialSolutionAlphametic)obj;
				reason = null;

				psa.LeadingZeroesChecked = true;

				// Neither of the addends can be longer than the sum
				if (psa.Add1.Length > psa.Sum.Length ||
					psa.Add2.Length > psa.Sum.Length)
				{
					fImpossible = true;
				}
				else
				{
					// One of them must be either the length of the sum or one smaller
					// than the length of the sum
					fImpossible = !(
						psa.Add1.Length == psa.Sum.Length ||
						psa.Add1.Length == psa.Sum.Length - 1 ||
						psa.Add2.Length == psa.Sum.Length ||
						psa.Add2.Length == psa.Sum.Length - 1);

				}
				return false;
			}
		}

		// The main rule that checks that the addition is correct.
		class ApplyAdditionRule : IRule
		{
			public bool FTrigger(IPartialSolution ps)
			{
				return true;
			}

			public bool FApply(IPartialSolution obj, out List<IReason> reason, out bool fImpossible)
			{
				var psa = (PartialSolutionAlphametic) obj;

				fImpossible = false;
				reason = null;
				var ret = false;

				// For each column in the sum
				// We go backward through the sum to take advantage of the fact that
				// carries move to the left...
				for (var i = 0; i < psa.Sum.Length; i++)
				{
					var notSet = Member.MemberCount;
					bool noAction = false;

					// For each member of the sum
					for (var memberVal = 0; memberVal < (int)Member.MemberCount; memberVal++)
					{
						var member = (Member) memberVal;
						if (psa.ValueAt(member, i) == PartialSolutionAlphametic.NoValue)
						{
							if (notSet != Member.MemberCount)
							{
								// Found two unset chars so nothing to do in this column
								noAction = true;
								break;
							}
							notSet = member;
						}
					}

					// If we found two unset values in this column just continue on to the next
					if (noAction)
					{
						continue;
					}

					// NOTE: We could keep track of which columns have been checked out in the
					// PartialSolutionAlphametic and save ourselves doing this check multiple
					// times.  Probably not worth it in this simple case.

					// If all members were set, check them to ensure they add properly
					if (notSet == Member.MemberCount)
					{
						var sum = psa.ValueAt(Member.Add1, i) +
						          psa.ValueAt(Member.Add2, i) +
						          psa.ValueAt(Member.Carry, i);
						if (sum % 10 != psa.ValueAt(Member.Sum, i))
						{
							// Didn't pan out - this is a dead end.
							fImpossible = true;
							return false;
						}
						var nextCarry = psa.ValueAt(Member.Carry, i + 1);
						if (nextCarry == PartialSolutionAlphametic.NoValue)
						{
							psa.Carries[i + 1] = (byte)(sum / 10);
						}
						else if (nextCarry != sum / 10)
						{
							// Carries don't match correctly
							fImpossible = true;
							return false;
						}
					}
					else
					{
						ret = true;
						if (notSet == Member.Carry)
						{
							psa.Carries[i] =
								(byte)((psa.ValueAt(Member.Sum, i) -
								 psa.ValueAt(Member.Add1, i) -
								 psa.ValueAt(Member.Add2, i) + 20) % 10);
							if (psa.Carries[i] != 0 && psa.Carries[i] != 1)
							{
								fImpossible = true;
								return false;
							}
						}
						else
						{
							char ch = '\0';
							byte val = PartialSolutionAlphametic.NoValue;

							// Exactly one unset member so we can calculate it's value
							switch (notSet)
							{
								case Member.Add1:
									ch = psa.Add1[psa.Add1.Length - 1 - i];
									val =
										(byte)((psa.ValueAt(Member.Sum, i) -
										 psa.ValueAt(Member.Add2, i) -
										 psa.ValueAt(Member.Carry, i) + 20) % 10);
									break;
								case Member.Add2:
									ch = psa.Add2[psa.Add2.Length - 1 - i];
									val =
										(byte)((psa.ValueAt(Member.Sum, i) -
										 psa.ValueAt(Member.Add1, i) -
										 psa.ValueAt(Member.Carry, i) + 20) % 10);
									break;

								case Member.Sum:
									ch = psa.Sum[psa.Sum.Length - 1 - i];
									val =
										(byte)((psa.ValueAt(Member.Add1, i) +
										 psa.ValueAt(Member.Add2, i) +
										 psa.ValueAt(Member.Carry, i)) % 10);
									break;
							}
							if (!psa.Possible[ch][val])
							{
								fImpossible = true;
								return false;
							}
							psa[ch] = val;

						}
						var sum = psa.ValueAt(Member.Add1, i) +
						                psa.ValueAt(Member.Add2, i) +
						                psa.ValueAt(Member.Carry, i);

						var nextCarry = psa.ValueAt(Member.Carry, i + 1);
						if (nextCarry == PartialSolutionAlphametic.NoValue)
						{
							psa.Carries[i + 1] = (byte)(sum / 10);
						}
						else if (nextCarry != sum / 10)
						{
							// Carries don't match correctly
							fImpossible = true;
							return false;
						}
					}
				}
				return ret;
			}
		}
	}
}
