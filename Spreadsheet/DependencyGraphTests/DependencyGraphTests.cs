namespace DependencyGraphTests;

using DependencyGraph;

/// <summary>
///   This is a test class for DependencyGraphTest and is intended
///   to contain all DependencyGraphTest Unit Tests
/// </summary>
/// <author>
/// Canyon Wirthlin
/// </author>
/// <date>
/// 9/12/2025
/// </date>
[TestClass]
public class DependencyGraphTests
{
  /// <summary>
  ///   Creates 200 nodes that have roughly 20k connections, then removes every 4th and adds every other back
  /// into the structure, then removes more (every 3rd for every even i), and then ensures it creates an answer key
  /// it creates to ensure 0 data loss.
  /// </summary>
  [TestMethod]
  [Timeout( 2000 )]  // 2 second run time limit
  public void StressTest()
  {
    DependencyGraph dg = new();

    // A bunch of strings to use
    const int SIZE = 200;
    string[] letters = new string[SIZE];
    for ( int i = 0; i < SIZE; i++ )
    {
      letters[i] = string.Empty + ( (char) ( 'a' + i ) );
    }

    // The correct answers
    HashSet<string>[] dependents = new HashSet<string>[SIZE];
    HashSet<string>[] dependees = new HashSet<string>[SIZE];
    for ( int i = 0; i < SIZE; i++ )
    {
      dependents[i] = [];
      dependees[i] = [];
    }

    // Add a bunch of dependencies
    for ( int i = 0; i < SIZE; i++ )
    {
      for ( int j = i + 1; j < SIZE; j++ )
      {
        dg.AddDependency( letters[i], letters[j] );
        dependents[i].Add( letters[j] );
        dependees[j].Add( letters[i] );
      }
    }

    // Remove a bunch of dependencies
    for ( int i = 0; i < SIZE; i++ )
    {
      for ( int j = i + 4; j < SIZE; j += 4 )
      {
        dg.RemoveDependency( letters[i], letters[j] );
        dependents[i].Remove( letters[j] );
        dependees[j].Remove( letters[i] );
      }
    }

    // Add some back
    for ( int i = 0; i < SIZE; i++ )
    {
      for ( int j = i + 1; j < SIZE; j += 2 )
      {
        dg.AddDependency( letters[i], letters[j] );
        dependents[i].Add( letters[j] );
        dependees[j].Add( letters[i] );
      }
    }

    // Remove some more
    for ( int i = 0; i < SIZE; i += 2 )
    {
      for ( int j = i + 3; j < SIZE; j += 3 )
      {
        dg.RemoveDependency( letters[i], letters[j] );
        dependents[i].Remove( letters[j] );
        dependees[j].Remove( letters[i] );
      }
    }

    // Make sure everything is right
    for ( int i = 0; i < SIZE; i++ )
    {
      Assert.IsTrue( dependents[i].SetEquals( new HashSet<string>( dg.GetDependents( letters[i] ) ) ) );
      Assert.IsTrue( dependees[i].SetEquals( new HashSet<string>( dg.GetDependees( letters[i] ) ) ) );
    }
  }

  [TestMethod]
  public void DependencyGraph_EmptyGetDependees_Valid()
  {
    var dg = new DependencyGraph();
    var list = dg.GetDependees("A1");
    Assert.IsFalse(list.Any());
  }

  [TestMethod]
  public void DependencyGraph_EmptyGetDependents_Valid()
  {
    var dg = new DependencyGraph();
    var list = dg.GetDependents("A1");
    Assert.IsFalse(list.Any());
  }

  [TestMethod]
  public void DependencyGraph_GetDependentsFull_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1", "A2");
    var list = dg.GetDependents("A1");
    CollectionAssert.AreEqual(new[]{"A2"}, list.ToList());
  }

  [TestMethod]
  public void DependencyGraph_GetDependeesFull_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    var list = dg.GetDependees("A2");
    CollectionAssert.AreEqual(new[]{"A1"}, list.ToList());
  }

  [TestMethod]
  public void DependencyGraph_EmptySize_Valid()
  {
    var dg = new DependencyGraph();
    Assert.AreEqual(0, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_FullSize_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    dg.AddDependency("A3","A4");
    dg.AddDependency("A5","A6");
    Assert.AreEqual(3, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_AddSize_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    Assert.AreEqual(1, dg.Size);
    dg.AddDependency("A2","A3");
    Assert.AreEqual(2, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_RemoveSize_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    Assert.AreEqual(1, dg.Size);
    dg.RemoveDependency("A1","A2");
    Assert.AreEqual(0, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_RemoveNonexistent_Valid()
  {
    var dg = new DependencyGraph();
    dg.RemoveDependency("A1","A2");
    Assert.AreEqual(0, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_AddDuplicateSameSize_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    dg.AddDependency("A1","A2");
    Assert.AreEqual(1, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_RemoveDuplicateSameSize_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    dg.AddDependency("A1","A2");
    Assert.AreEqual(1, dg.Size);
    dg.RemoveDependency("A1","A2");
    Assert.AreEqual(0, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_ReplaceDependentsSameSize_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    dg.AddDependency("A1","A3");
    Assert.AreEqual(2, dg.Size);
    dg.ReplaceDependents("A1", ["B1", "B2", "B3"]);
    Assert.AreEqual(3, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_ReplaceNonexistentDependents_Valid()
  {
    var dg = new DependencyGraph();
    dg.ReplaceDependents("A1", ["B1", "B2", "B3"]);
    Assert.AreEqual(3, dg.Size);
  }
  [TestMethod]
  public void DependencyGraph_ReplaceWithEmptyDependents_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A2","A1");
    dg.AddDependency("A2","A3");
    Assert.AreEqual(2, dg.Size);
    dg.ReplaceDependents("A2",[]);
  }
  [TestMethod]
  public void DependencyGraph_ReplaceDependentsDuplicates_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    dg.ReplaceDependents("A1",  ["A2", "A2", "A2", "B2"]);
    Assert.AreEqual(2, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_ReplaceDependeesDuplicates_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    dg.ReplaceDependees("A2",  ["A1", "A1", "A1", "B2"]);
    Assert.AreEqual(2, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_ReplaceDependeesSameSize_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A2","A1");
    dg.AddDependency("A3","A1");
    Assert.AreEqual(2, dg.Size);
    dg.ReplaceDependees("A1", ["B1", "B2", "B3"]);
    Assert.AreEqual(3, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_ReplaceNonexistentDependees_Valid()
  {
    var dg = new DependencyGraph();
    dg.ReplaceDependees("A1", ["B1", "B2", "B3"]);
    Assert.AreEqual(3, dg.Size);
  }

  [TestMethod]
  public void DependencyGraph_ReplaceWithEmptyDependees_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    dg.AddDependency("A3","A2");
    Assert.AreEqual(2, dg.Size);
    dg.ReplaceDependees("A2",[]);
  }

  [TestMethod]
  public void DependencyGraph_HasDependeesEmpty_Valid()
  {
    var dg = new DependencyGraph();
    Assert.IsFalse(dg.HasDependees("A1"));
  }

  [TestMethod]
  public void DependencyGraph_HasDependentsEmpty_Valid()
  {
    var dg = new DependencyGraph();
    Assert.IsFalse(dg.HasDependents("A1"));
  }

  [TestMethod]
  public void DependencyGraph_HasDependentsFull_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    dg.AddDependency("A3","A4");
    Assert.IsTrue(dg.HasDependents("A3"));
  }

  [TestMethod]
  public void DependencyGraph_HasDependeesFull_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A2");
    dg.AddDependency("A3","A4");
    Assert.IsTrue(dg.HasDependees("A4"));
  }

  [TestMethod]
  public void DependencyGraph_AddSelf_Valid()
  {
    var dg = new DependencyGraph();
    dg.AddDependency("A1","A1");
    Assert.AreEqual(1, dg.Size);
  }
}