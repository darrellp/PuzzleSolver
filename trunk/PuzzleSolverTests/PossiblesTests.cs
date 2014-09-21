using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PuzzleSolverTests
{
	[TestClass]
	public class PossiblesTests
	{
		[TestMethod]
		public void TestToString()
		{
			var p = new Possibles("m", 3);
			Assert.AreEqual("m = {0,1}", p.ToString());
			p.Disallow(1);
			Assert.AreEqual("m = 0", p.ToString());
			p.Disallow(0);
			Assert.AreEqual("m: Impossible", p.ToString());
		}

		[TestMethod]
		public void TestNormalPossibles()
		{
			var possibleSum = new Possibles("S");			// All possibilities
			var possibleAdd1 = new Possibles("?", 1);		// Fixed at 0
			var possibleAdd2 = new Possibles("?", 1);		// Fixed at 0
			var possibleCarry = Possibles.Carry(3);			// 0 or 1
			bool fCarry0, fCarry1;
			var sum = possibleAdd1.Add(possibleAdd2, out fCarry0, out fCarry1);
			Assert.IsTrue(fCarry0);
			Assert.IsFalse(fCarry1);
			sum = possibleCarry.Add(sum, out fCarry0, out fCarry1);
			Assert.IsTrue(fCarry0);
			Assert.IsFalse(fCarry1);
			possibleSum.Set(sum);
			Assert.AreEqual(3, possibleSum.Values);
			sum = Possibles.Add(0x155, 0x2, out fCarry0, out fCarry1);				// Add 1 to evens
			Assert.IsTrue(fCarry0);
			Assert.IsFalse(fCarry1);
			Assert.AreEqual(sum, 0x2AA);					// Should add to odds
			sum = Possibles.Add(0x155, 0x2, out fCarry0, out fCarry1, true);		// Subtract 1
			Assert.IsTrue(fCarry0);
			Assert.IsFalse(fCarry1);
			Assert.AreEqual(sum, 0x2AA);					// Should add to odds
			sum = Possibles.Add(0x155, 0x10, out fCarry0, out fCarry1);				// Add 4
			Assert.AreEqual(sum, 0x155);					// Adds to evens
			sum = Possibles.Add(0x30C, 0x18, out fCarry0, out fCarry1);				// {2,3,8,9} + {3,4}
			Assert.AreEqual(sum, 0xEE);						// {1,2,3,5,6,7}
			sum = Possibles.Add(0x18, 0x30C, out fCarry0, out fCarry1);				// {3,4} + {2,3,8,9}
			Assert.AreEqual(sum, 0xEE);						// {1,2,3,5,6,7}
		}
	}
}
