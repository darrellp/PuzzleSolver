namespace PuzzleSolver
{
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Used to generate puzzles </summary>
	///
	/// <remarks>	Darrellp, 2/14/2011. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public abstract class Generator<T> where T : IPartialSolution, new()
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	A list of extensions which gives a complete solutions from an empty one.. </summary>
		///
		/// <returns>	The list of extensions. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		protected abstract IExtension[] ExtensionList();
		IExtension[] _arext;
	    readonly T _psFinal = new T();

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Generates a puzzle based on a passed in expert system</summary>
		///
		/// <param name="e"> The expert system to generate from. </param>
		///
		/// <returns>	. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public T Generate(ExpertSystem<T> e)
		{
			_arext = ExtensionList();

			var iSplit = _arext.Length + 1;

			while (true)
			{
				iSplit = ISplitBinarySearch(0, iSplit, e);
				_psFinal.PsApply(_arext[iSplit - 1], false);
				if (Backtracker<T>.FUnique(_psFinal, e) || iSplit == 1)
				{
					break;
				}
			}
			return _psFinal;
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

			var iProbe = (iUnique + iNonUnique) / 2;
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
			var ps = (T)_psFinal.Clone();

			for (var i = 0; i < iPop; i++)
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
