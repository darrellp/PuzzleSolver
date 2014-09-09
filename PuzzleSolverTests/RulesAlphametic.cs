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
				char chAdd1 = psa.Add1[0];
				char chAdd2 = psa.Add2[0];
				char chSum = psa.Sum[0];
				psa.SetImpossible(chAdd1, 0);
				psa.SetImpossible(chAdd2, 0);
				psa.SetImpossible(chSum, 0);
				fImpossible = false;
				reason = new List<IReason>
				{
					new DisableLeadingZero(chAdd1),
					new DisableLeadingZero(chAdd2),
					new DisableLeadingZero(chSum)
				};
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
				if (fImpossible)
				{
					reason = new List<IReason>
					{
						new ImpossibleSizes()
					};
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
				reason = new List<IReason>();
				var ret = false;

				// For each column in the sum
				// We go backward through the sum to take advantage of the fact that
				// carries move to the left...
				for (var i = 0; i < psa.Sum.Length; i++)
				{
					Member notSet;
					var cv = psa.GetColumnValues(i);

					// If we found two unset values in this column just continue on to the next
					if (CheckSetValues(cv, out notSet))
					{
						continue;
					}

					// NOTE: We could keep track of which columns have been checked out in the
					// PartialSolutionAlphametic and save ourselves doing this check multiple
					// times.  Probably not worth it in this simple case.

					// If all members were set, check them to ensure they add properly
					if (notSet == Member.MemberCount)
					{
						fImpossible = CheckAddition(reason, cv, psa, i);
						if (fImpossible)
						{
							return false;
						}
					}
					else
					{
						ret = true;
						if (notSet == Member.Carry)
						{
							psa.Carries[i] = (byte)((cv.Sum - cv.Add1 - cv.Add2 + 20) % 10);
							if (psa.Carries[i] != 0 && psa.Carries[i] != 1)
							{
								reason.Add(new ImpossibleSum(i, cv.Carry, cv.Add1, cv.Add2, cv.Sum));
								fImpossible = true;
								return false;
							}
							reason.Add(new ReasonSum(i, cv.Carry, cv.Add1, cv.Add2, cv.Sum, psa.Carries[i]));
						}
						else
						{
							// Exactly one unset member so we can calculate it's value
							byte val;

							var ch = DetermineUnsetValue(notSet, psa, i, cv, out val);
							if (!psa.Possible[ch][val])
							{
								reason.Add(new ImpossibleSum(i, cv.Carry, cv.Add1, cv.Add2, cv.Sum));
								fImpossible = true;
								return false;
							}
							reason.Add(new ReasonSum(i, cv.Carry, cv.Add1, cv.Add2, cv.Sum, val));
							psa[ch] = val;

						}
						var sum = psa.ValueAt(Member.Add1, i) +
						                psa.ValueAt(Member.Add2, i) +
						                psa.ValueAt(Member.Carry, i);

						var nextCarry = psa.ValueAt(Member.Carry, i + 1);
						if (nextCarry == PartialSolutionAlphametic.NoValue)
						{
							reason.Add(new GenerateCarry(i + 1, (byte)(sum / 10)));
							psa.Carries[i + 1] = (byte)(sum / 10);
						}
						else if (nextCarry != sum / 10)
						{
							// Carries don't match correctly
							reason.Add(new ImpossibleCarry(i + 1, (byte)(sum / 10)));
							fImpossible = true;
							return false;
						}
					}
				}
				return ret;
			}

			private static char DetermineUnsetValue(Member notSet, PartialSolutionAlphametic psa, int i, ColumnValues cv, out byte val)
			{
				char ch = '\0';
				val = PartialSolutionAlphametic.NoValue;
				switch (notSet)
				{
					case Member.Add1:
						ch = psa.Add1[psa.Add1.Length - 1 - i];
						val = (byte) ((cv.Sum - cv.Add2 - cv.Carry + 20) % 10);
						break;

					case Member.Add2:
						ch = psa.Add2[psa.Add2.Length - 1 - i];
						val = (byte) ((cv.Sum - cv.Add1 - cv.Carry + 20) % 10);
						break;

					case Member.Sum:
						ch = psa.Sum[psa.Sum.Length - 1 - i];
						val = (byte) ((cv.Add1 + cv.Add2 + cv.Carry) % 10);
						break;
				}
				return ch;
			}

			private static bool CheckAddition(List<IReason> reason, ColumnValues cv, PartialSolutionAlphametic psa,
				int i)
			{
				bool fImpossible = false;
				var sum = cv.Add1 + cv.Add2 + cv.Carry;
				var nextCarry = psa.ValueAt(Member.Carry, i + 1);
				if (sum % 10 != cv.Sum)
				{
					// Didn't pan out - this is a dead end.
					reason.Add(new ImpossibleSum(i, cv.Carry, cv.Add1, cv.Add2, cv.Sum));
					fImpossible = true;
				}
				else if (nextCarry == PartialSolutionAlphametic.NoValue)
				{
					reason.Add(new GenerateCarry(i + 1, (byte) (sum / 10)));
					psa.Carries[i + 1] = (byte) (sum / 10);
				}
				else if (nextCarry != sum / 10)
				{
					// Carries don't match correctly
					reason.Add(new ImpossibleCarry(i + 1, (byte) (sum / 10)));
					fImpossible = true;
				}
				return fImpossible;
			}

			private static bool CheckSetValues(ColumnValues values, out Member notSet)
			{
				var noAction = false;
				notSet = Member.MemberCount;

				// For each member of the sum
				for (var memberVal = 0; memberVal < (int)Member.MemberCount; memberVal++)
				{
					if (values[memberVal] == PartialSolutionAlphametic.NoValue)
					{
						if (notSet != Member.MemberCount)
						{
							// Found two unset chars so nothing to do in this column
							noAction = true;
							break;
						}
						notSet = (Member)memberVal;
					}
				}
				return noAction;
			}
		}
	}
}
