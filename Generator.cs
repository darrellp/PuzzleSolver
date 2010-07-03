using System;
using System.Collections.Generic;
using System.Text;

namespace PuzzleSolver
{
	abstract public class Generator<T> where T : IPartialSolution, new()
	{
		protected abstract IExtension[] ExtensionList();
		IExtension[] _arext;
		T psFinal = new T();

		public T Generate(ExpertSystem<T> e)
		{
			_arext = ExtensionList();

			int iSplit = _arext.Length + 1;

			while (true)
			{
				iSplit = ISplitBinarySearch(0, iSplit, e);
				//Console.WriteLine(">>>> Final Solution gets index {0}: {1}", iSplit, _arext[iSplit].ToString());
				psFinal.PsApply(_arext[iSplit - 1], false);
				if (Backtracker<T>.FUnique(psFinal, e) || iSplit == 1)
				{
					break;
				}
			}
			return psFinal;
		}

		/// <summary>
		/// Find the point, n, at which applying all extensions less than n yields a puzzle with
		/// a non-unique solution but including extension n yields a puzzle with a unique solution.
		/// 
		/// Note: It is not quite invariant that iNonUnique is in fact non-unique.  We initially call
		/// this function with zero which is normally non-unique since it represents a single placement
		/// but since this call continually adds in elements, towards the end, that single placement
		/// may be necessary to achieve uniqueness.
		/// </summary>
		/// <param name="iNonUnique"></param>
		/// <param name="iUnique"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		int ISplitBinarySearch(int iNonUnique, int iUnique, ExpertSystem<T> e)
		{
			//Console.Write("iNonUnique: {0} iUnique {1} ", iNonUnique, iUnique);
			if (iUnique == iNonUnique + 1)
			{
				//Console.WriteLine();
				return iUnique;
			}

			int iProbe = (iUnique + iNonUnique) / 2;
			//Console.WriteLine("iProbe: {0}", iProbe);
			if (FIsUnique(iProbe, e))
			{
				iUnique = iProbe;
			}
			else
			{
				iNonUnique = iProbe;
			}

			return ISplitBinarySearch(iNonUnique, iUnique, e);
		}

		T PsAtIndex(int iPop)
		{
			T ps = (T)psFinal.Clone();

			for (int i = 0; i < iPop; i++)
			{
				ps.PsApply(_arext[i], false);
			}

			return ps;
		}

		bool FIsUnique(int i, ExpertSystem<T> e)
		{
			//T psNext = PsAtIndex(i);
			//bool fRet = Backtracker<T>.FUnique(psNext, e);
			//Console.WriteLine("The following board is {0}", fRet ? "Unique" : "Non-unique");
			//Console.WriteLine("{0}", psNext.ToString());
			//return fRet;
			return Backtracker<T>.FUnique(PsAtIndex(i), e);
		}
	}
}
