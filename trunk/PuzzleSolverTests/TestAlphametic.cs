using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuzzleSolver;

namespace PuzzleSolverTests
{
	[TestClass]
	public class TestAlphametic
	{
		[TestMethod]
		public void TestSendMoreMoney()
		{
			var psa = new PartialSolutionAlphametic("SEND", "MORE", "MONEY");
			var es = new ExpertSystem<PartialSolutionAlphametic>(
				RulesAlphametic.Rules, 
				RulesAlphametic.OneTimeRules);
			PartialSolutionAlphametic psaSolved;
			bool fSolved = Backtracker<PartialSolutionAlphametic>.FSolve(psa, es, out psaSolved);
			Assert.IsTrue(fSolved);
			Assert.AreEqual(psaSolved['S'], 9);
			Assert.AreEqual(psaSolved['E'], 5);
			Assert.AreEqual(psaSolved['N'], 6);
			Assert.AreEqual(psaSolved['D'], 7);
			Assert.AreEqual(psaSolved['M'], 1);
			Assert.AreEqual(psaSolved['O'], 0);
			Assert.AreEqual(psaSolved['R'], 8);
			Assert.AreEqual(psaSolved['Y'], 2);
		}
	}
}
