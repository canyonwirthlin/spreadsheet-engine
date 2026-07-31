
// <copyright file="FormulaSyntaxTests.cs" company="UofU-CS3500">
//   Copyright 2024 UofU-CS3500. All rights reserved.
// </copyright>

using System.Text;

namespace FormulaTests;

using Formula;

/// <summary>
///   <para>
///     The following class tests the Formula class's public methods for correctness
///   </para>
/// </summary>
/// <author>
/// Canyon Wirthlin
/// </author>
/// <date>
/// 9/19/2025
/// </date>
[TestClass]
public class FormulaTests
{
    // --- Tests for One Token Rule ---
    [TestMethod]
    public void FormulaConstructor_TestNoTokens_Invalid( )
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(string.Empty));
    }

    [TestMethod]
    [DataRow("a1a")]
    [DataRow("a")]
    [DataRow("1e")]
    [DataRow("one")]
    [DataRow("-5")]
    public void FormulaConstructor_TestBadToken_Invalid(string formula)
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(formula));
    }
    
    [TestMethod]
    [DataRow("1")]
    [DataRow("3.14")]
    [DataRow("5e2")]
    public void FormulaConstructor_TestTokens_Valid(string formula)
    {
        _ = new Formula(formula);
    }

    // --- Tests for Valid Token Rule ---
    [TestMethod]
    public void FormulaConstructor_TestGoodAndBadTokens_Invalid()
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula("1 + 2a"));
    }
    
    [TestMethod]
    public void FormulaConstructor_TestValidTokens_Valid()
    {
        _ = new Formula("A2 + B2");
    }

    /// <summary>
    /// Tests each printable char on the ascii table besides the valid tokens.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_InvalidSymbols_Invalid()
    {
        const string validChars = "0123456789+-*/()abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ";
        var errors = new List<string>();
        for (var i = 32; i < 127; i++)
        {
            var op = (char)i;
            if (validChars.Contains(op))
            {
                continue;
            }

            try
            {
                _ = new Formula($"1 {op} 2");
                errors.Add($"Expected exception not thrown for char: '{op}'");
            }
            catch (FormulaFormatException)
            {
            }
                
            if (errors.Count != 0)
            {
                Assert.Fail(string.Join("\n", errors));
            }
        }
    }
    
    // --- Tests for Closing Parenthesis Rule
    [TestMethod]
    [DataRow(")(1 + 2")]
    [DataRow(")1 + 2(")]
    [DataRow("1+2)")]
    [DataRow("(1+2")]
    [DataRow("(+1)")]
    public void FormulaConstructor_IncorrectParenthesisPlacement_Invalid(string formula)
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(formula));
    }
    
    // --- Tests for Balanced Parentheses Rule
    [TestMethod]
    [DataRow(")))(((1")]
    [DataRow("((1+2)")]
    [DataRow("((a2)*6) - 1)")]
    public void FormlaConstructor_UnbalancedParenthesisPlacement_Invalid(string formula)
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(formula));
    }

    [TestMethod]
    [DataRow("((a1) - 2) * 3")]
    [DataRow("(((((15)))))")]
    [DataRow("(((((aa1)))))")]
    public void FormulaConstructor_BalancedParenthesisPlacement_Valid(string formula)
    {
        _  = new Formula(formula);
    }
    
    // --- Tests for First Token Rule
    [TestMethod]
    [DataRow("1+2")]
    [DataRow("a1 + a2")]
    [DataRow("(a2) * a1 - 3 + 4")]
    [DataRow("((a2-2) + 3) * a1 - 3 + 4")]
    public void FormulaConstructor_TestFirstTokenNumber_Valid(string formula)
    {
        _ = new Formula(formula);
    }

    [TestMethod]
    [DataRow("-1+2")]
    [DataRow("+a2")]
    [DataRow("/2")]
    [DataRow("*2")]
    public void FormulaConstructor_BadFirstToken_Invalid(string formula)
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(formula));
    }
    
    // --- Tests for  Last Token Rule ---
    [TestMethod]
    [DataRow("1)")]
    [DataRow("1/")]
    [DataRow("2*")]
    [DataRow("2+")]
    [DataRow("2 + 1e")]
    [DataRow("2 + a1a")]
    [DataRow("2 + a")]
    [DataRow("2 + one")]
    public void FormulaConstructor_BadLastToken_Invalid(string formula)
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(formula));
    }

    [TestMethod]
    [DataRow("2 + 3e5")]
    public void FormulaConstructor_ValidLastTokens_Valid(string formula)
    {
        _ = new Formula(formula);
    }
    
    // --- Tests for Parentheses/Operator Following Rule ---
    [TestMethod]
    [DataRow("(+1)")]
    [DataRow("(-2)")]
    [DataRow("(/2)")]
    [DataRow("(*2)")]
    [DataRow("5++5")]
    public void FormulaConstructor_OpeningParenthesisFollowedByOperatorToken_Invalid(string formula)
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(formula));
    }
    
    // --- Tests for Extra Following Rule ---
    [TestMethod]
    [DataRow("(2)1")]
    [DataRow("(2))")]
    [DataRow("A1+")]
    public void FormulaConstructor_TokenFollowedByWrongToken_Invalid(string formula)
    {
        Assert.Throws<FormulaFormatException>(() => _ = new Formula(formula));
    }
    
    // Get Variables Tests
    [TestMethod]
    [DataRow("X1+x1+Z1+C1",3)]
    [DataRow("aa1+a1+aaa1+Aa1+aAa1",3)]
    public void GetVariables_UpperLowerSame_Valid(string formula, int count) {
        var f = new Formula(formula).GetVariables();
        Assert.AreEqual(count, f.Count);
    }
    
    [TestMethod]
    public void GetVariables_NoVariablesReturnsEmpty_Valid()
    {
        var f = new Formula("5 + 3 * (2 - 1) / 2");
        Assert.IsEmpty(f.GetVariables());
    }
    
    [TestMethod]
    public void GetVariables_OrderIsConsistent_Valid()
    {
        var f = new Formula("z1 + x1 + y1");
        var vars = f.GetVariables().ToList();
        CollectionAssert.AreEqual(new[] { "Z1", "X1", "Y1" }, vars);
    }
    
    [TestMethod]
    public void GetVariables_SeparatesScientificNotation_Valid()
    {
        var f = new Formula("ee4 -  (2 * e2) + 2e2");
        Assert.HasCount(2, f.GetVariables());
    }
    
    // ToString Tests
    [TestMethod]
    public void ToString_AllLowerCase_Valid()
    {
        var f = new Formula("x1+x1+a1");
        Assert.AreEqual("X1+X1+A1", f.ToString());
    }
    
    [TestMethod]
    [DataRow("x1 + 5.0000","X1+5")]
    [DataRow("05.00 - 005","5-5")]
    [DataRow("03.50 - 0.5","3.5-0.5")]
    public void ToString_NormalizesNumbers_Valid(string formula, string expected)
    {
        var f = new Formula(formula);
        Assert.AreEqual(expected, f.ToString());
    }

    [TestMethod]
    public void ToString_IgnoresWhitespace_Valid()
    {
        var f = new Formula("x1      + 2");
        Assert.AreEqual("X1+2", f.ToString());
    }
    
    [TestMethod]
    public void ToString_KeepsAllOperators_Valid()
    {
        var f = new Formula("(a1+1-2*ad1/4)");
        Assert.AreEqual("(A1+1-2*AD1/4)", f.ToString());
    }

    [TestMethod]
    [DataRow("(a1+2e4)","(A1+20000)")]
    [DataRow("1.23E-2","0.0123")]
    public void ToString_NormalizesScientificNotation_Valid(string formula, string expected)
    {
        var f = new Formula(formula);
        Assert.AreEqual(expected, f.ToString());
    }

    [TestMethod]
    public void ToString_ToStringToFormula_Valid()
    {
        var f = new Formula("(a1+2e4)");
        var g = new Formula(f.ToString());
        Assert.AreEqual("(A1+20000)", g.ToString());
    }
    
    // equals
    
    [TestMethod]
    public void Formula_EqualsSame_Valid()
    {
        var f1 = new Formula("A1+B1");
        var f2 = new Formula("A1+B1");
        Assert.IsTrue(f1 == f2);
    }
    
    [TestMethod]
    public void Formula_EqualsInverse_Invalid()
    {
        var f1 = new Formula("A1+B1");
        var f2 = new Formula("B1+A1");
        Assert.IsFalse(f1 == f2);
    }
    
    [TestMethod]
    public void Formula_EqualsSameValue_Invalid()
    {
        var f1 = new Formula("1 + 2");
        var f2 = new Formula("4 - 1");
        Assert.IsFalse(f1 == f2);
    }
    
    // not equals
    
    [TestMethod]
    public void Formula_NotEqualsSame_Invalid()
    {
        var f1 = new Formula("A1+B1");
        var f2 = new Formula("A1+B1");
        Assert.IsFalse(f1 != f2);
    }
    
    [TestMethod]
    public void Formula_NotEqualsInverse_Valid()
    {
        var f1 = new Formula("A1+B1");
        var f2 = new Formula("B1+A1");
        Assert.IsTrue(f1 != f2);
    }
    
    [TestMethod]
    public void Formula_NotEqualsSameValue_Valid()
    {
        var f1 = new Formula("1 + 2");
        var f2 = new Formula("4 - 1");
        Assert.IsTrue(f1 != f2);
    }
    
    // evaluate
    
    [TestMethod]
    public void Formula_EvaluateDoubleOverflow_Valid()
    {
        var formula = new Formula("1e308");
        double MyLookup(string s) => 3;
        Assert.AreEqual(1e308, formula.Evaluate(MyLookup));
    }
    
    [TestMethod]
    public void Formula_EvaluateDoubleUnderflow_Valid()
    {
        var formula = new Formula("1e-308");
        double MyLookup(string s) => 3;
        Assert.AreEqual(1e-308,formula.Evaluate(MyLookup));
    }
    
    [TestMethod]
    public void Formula_EvaluateSubtraction_Valid()
    {
        var formula = new Formula("17-1");
        double MyLookup(string s) => 3;
        Assert.AreEqual((double)16,formula.Evaluate(MyLookup));
    }
    
    [TestMethod]
    public void Formula_EvaluateAddition_Valid()
    {
        var formula = new Formula("17+1");
        double MyLookup(string s) => 3;
        Assert.AreEqual((double)18,formula.Evaluate(MyLookup));
    }

    [TestMethod]
    public void Formula_EvaluateDivisionValid()
    {
        var formula = new Formula("2/0.5");
        double MyLookup(string s) => 3;
        Assert.AreEqual((double)4,formula.Evaluate(MyLookup));
    }

    [TestMethod]
    public void Formula_EvaluateMultiplication_Valid()
    {
        var formula = new Formula("0.5*2");
        double MyLookup(string s) => 3;
        Assert.AreEqual((double)1,formula.Evaluate(MyLookup));
    }
    
    [TestMethod]
    public void Formula_EvaluateComplexParenthesis_Valid()
    {
        var formula = new Formula("(0)-2*(250)*(2)");
        double MyLookup(string s) => 3;
        Assert.AreEqual((double)-1000,formula.Evaluate(MyLookup));
    }
    
    [TestMethod]
    public void Formula_EvaluateOperatorPrecedence_Valid()
    {
        var formula = new Formula("2 + 3 * (4 + 5) - 6 / 3");
        double MyLookup(string s) => 3;
        Assert.AreEqual((double)27,formula.Evaluate(MyLookup));
    }
    
    [TestMethod]
    public void Formula_EvaluateDifferentVariables_Valid()
    {
        var formula = new Formula("A1 + A2");
        double MyLookup(string s) => double.Parse(s[1].ToString());
        Assert.AreEqual((double)3,formula.Evaluate(MyLookup));
    }
    
    [TestMethod]
    public void Formula_EvaluateDivideByZero_ReturnsError()
    {
        var formula = new Formula("1/A1");
        double MyLookup(string s) => 0;
        var result = formula.Evaluate(MyLookup);
        Assert.IsTrue(result is FormulaError, result.ToString() ?? "No result");
    }
    
    [TestMethod]
    public void Formula_EvaluateEmptyVariable_ReturnsError()
    {
        var formula = new Formula("1/A1");
        double MyLookup(string s) => throw new ArgumentException();
        var result = formula.Evaluate(MyLookup);
        Assert.IsTrue(result is FormulaError, result.ToString() ?? "No result");
    }
    /// <summary>
    /// This method tests to ensure the formula class doesn't take longer than expected to create large formulas with lots of tokens
    /// </summary>
    [TestMethod]
    [Timeout(2000)]
    public void Formula_StressTest_Valid()
    {
        var size = 9999;
        var sb = new StringBuilder();
        for (int i = 1; i < size; i++)
            sb.Append("1 +");
        sb.Append('1');
        var formula = new Formula(sb.ToString());
        double MyLookup(string s) => 0;
        Assert.AreEqual((double)size,formula.Evaluate(MyLookup));
    }
    
    // gethashcode
    
    [TestMethod]
    public void Formula_GetHashCodeSame_Valid()
    {
        var f1 = new Formula("A1+B1");
        var f2 = new Formula("A1+B1");
        Assert.AreEqual(f2.GetHashCode(), f1.GetHashCode());
    }
    
    [TestMethod]
    public void Formula_GetHashCodeOverflow_Valid()
    {
        var f1 = new Formula("1e308");
        var f2 = new Formula("1e308");
        Assert.AreEqual(f2.GetHashCode(), f1.GetHashCode());
    }
    
    [TestMethod]
    public void Formula_GetHashCodeUnderflow_Valid()
    {
        var f1 = new Formula("1e-308");
        var f2 = new Formula("1e-308");
        Assert.AreEqual(f2.GetHashCode(), f1.GetHashCode());
    }
    
    [TestMethod]
    public void Formula_GetHashCodeSameValue_Valid()
    {
        var f1 = new Formula("50");
        var f2 = new Formula("25+25");
        Assert.AreNotEqual(f2.GetHashCode(), f1.GetHashCode());
    }
    
    [TestMethod]
    public void Formula_GetHashCodeWhiteSpace_Valid()
    {
        var f1 = new Formula("A1");
        var f2 = new Formula("    A1   ");
        Assert.AreEqual(f2.GetHashCode(), f1.GetHashCode());
    }
}