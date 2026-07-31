// Written by Canyon Wirthlin 10/17/25
using System.Globalization;

namespace SpreadsheetTests;
using Formula;
using Spreadsheets;

/// <summary>
/// Tests the spreadsheet class and it's public methods
/// </summary>
[TestClass]
public sealed class SpreadsheetTests
{
    //SetContentsOfCell(string name, double number)
    [TestMethod]
    public void SetContentsOfCell_Double_OneVar()
    {
        var ss = new Spreadsheet();
        var expected = new List<String>(new[] {"A1"});
        var result = ss.SetContentsOfCell("A1", double.MaxValue.ToString(CultureInfo.InvariantCulture)).ToList();
        CollectionAssert.AreEqual(expected,  result);
    }
    
    [TestMethod]
    public void SetContentsOfCell_MaxDouble_Valid()
    {
        var ss = new Spreadsheet();
        ss.SetContentsOfCell("A1", double.MaxValue.ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(double.MaxValue,  ss.GetCellContents("A1"));
    }
    //SetContentsOfCell(string name, string text)
    [TestMethod]
    public void SetContentsOfCell_EmptyString_Valid()
    {
        var ss = new Spreadsheet();
        ss.SetContentsOfCell("A1", string.Empty);
        Assert.AreEqual(string.Empty,  ss.GetCellContents("A1"));
    }
    //SetContentsOfCell(string name, Formula formula)
    [TestMethod]
    public void SetContentsOfCell_BadFormula_Invalid()
    {
        var ss = new Spreadsheet();
        Assert.ThrowsException<FormulaFormatException>(() => ss.SetContentsOfCell("A1", "="));
    }
    
    [TestMethod]
    public void SetContentsOfCell_Formula_CorrectList()
    {
        var ss = new Spreadsheet();
        var expected = new List<String>(new[] {"A1", "B1"});
        ss.SetContentsOfCell("B1", "=A1+2");
        var result = ss.SetContentsOfCell("A1", "22");
        CollectionAssert.AreEqual(expected,  result.ToList());
    }
    
    /// <summary>
    /// Creates a spreadsheet with a 26 level dependency graph
    /// to test the validity of SetContentsOfCell returned list
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_ComplexDependencies_CorrectList()
    {
        var ss = new Spreadsheet();
        for (int i = 1; i < 26; i++)
        {
            ss.SetContentsOfCell("A" + (i + 1), ("=A" + i)); 
        }

        var result = ss.SetContentsOfCell("A1", "2");
        var expected = new List<String>();
        for (int i = 1; i < 27; i++)
        {
            expected.Add($"A{i}");
        }
        CollectionAssert.AreEqual(expected,  result.ToList());
    }
    //GetNamesOfAllNonemptyCells()
    [TestMethod]
    public void GetNamesOfAllNonemptyCells_Empty_EmptySet()
    {
        var ss = new Spreadsheet();
        Assert.AreEqual(0, ss.GetNamesOfAllNonemptyCells().Count);
    }
    
    [TestMethod]
    public void GetNamesOfAllNonemptyCells_Populated_ValidSet()
    {
        var ss = new Spreadsheet();
        ss.SetContentsOfCell("A1", "0");
        ss.SetContentsOfCell("A2", "0");
        ss.SetContentsOfCell("A1", "1");
        ss.SetContentsOfCell("A2", "0.5");
        Assert.AreEqual(2, ss.GetNamesOfAllNonemptyCells().Count);
    }
    //
    //GetCellContents(string name)
    [TestMethod]
    public void GetCellContents_InvalidName_Exception()
    {
        var ss = new Spreadsheet();
        Assert.ThrowsException<InvalidNameException>(() => ss.GetCellContents("1a"));
    }
    [TestMethod]
    public void GetCellContents_Empty_Valid()
    {
        var ss = new Spreadsheet();
        Assert.AreEqual(string.Empty,  ss.GetCellContents("A1"));
    }
    [TestMethod]
    public void GetCellContents_Double_Valid()
    {
        var ss = new Spreadsheet();
        ss.SetContentsOfCell("A1", "3.14");
        Assert.AreEqual(3.14,  ss.GetCellContents("A1"));
    }
    [TestMethod]
    public void GetCellContents_String_Valid()
    {
        var ss = new Spreadsheet();
        ss.SetContentsOfCell("A1", "Hey");
        Assert.AreEqual("Hey",  ss.GetCellContents("A1"));
    }
    [TestMethod]
    public void GetCellContents_Formula_Valid()
    {
        var ss = new Spreadsheet();
        var formula = new Formula("(1+2) * 4");
        ss.SetContentsOfCell("A1","=(1+2)*4");
        Assert.AreEqual(formula,  ss.GetCellContents("A1"));
    }
    [TestMethod]
    public void GetCellContents_DirectCircularFormula_Invalid()
    {
        var ss = new Spreadsheet();
        Assert.ThrowsException<CircularException>(() => ss.SetContentsOfCell("A1", "=A1*2"));
    }
    [TestMethod]
    public void GetCellContents_IndirectCircularFormula_Invalid()
    {
        var ss = new Spreadsheet();
        ss.SetContentsOfCell("A2", "=A1*2");
        Assert.ThrowsException<CircularException>(() => ss.SetContentsOfCell("A1", "=A2*2"));
    }
}