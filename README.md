# Spreadsheet Application

A functional spreadsheet engine built from scratch in C# (.NET 9), developed across multiple assignments in CS 3500 (Software Practice) at the University of Utah.

## Architecture

The project demonstrates seperation of concern by being built in three layers, each depending on the one below it:

```
Spreadsheet  ←  depends on  →  Formula  ←  depends on  →  DependencyGraph
```

**DependencyGraph** 

a bidirectional directed graph tracking which cells depend on which. Each node stores both its `dependents` (things that depend on it) and `dependees` (things it depends on) as HashSets, making lookups O(1). Used by the spreadsheet to determine recalculation order and detect circular dependencies.

**Formula** 

an infix expression parser and evaluator. Tokenizes input using regex, validates syntax (balanced parentheses, valid variable names, operator rules), and evaluates with correct operator precedence. Variables (e.g. `A1`, `B2`) are resolved at evaluation time via a lookup delegate, allowing formulas to reference cell values dynamically.

**Spreadsheet** 

the top-level model. Cells can hold strings, doubles, or Formula objects. When a cell's value changes, the spreadsheet uses the DependencyGraph to find all transitively dependent cells and recalculates them in topological order. Circular dependencies throw a `CircularException`. The full spreadsheet state serializes to/from JSON.

## Key Technical Details

- **Circular dependency detection** via DFS traversal on the dependency graph before committing any cell update
- **Topological recalculation order** 
 
  when a cell changes, all dependents are recalculated in the correct order using `GetCellsToRecalculate`
- **Custom JSON serialization** 
  
  a `FormulaConverter` handles the ambiguity between strings and Formula objects during save/load, prefixing formula strings with `=`
- **Comprehensive unit tests** 
  
  separate test projects for each layer with both authored tests and provided grading tests

## Tech Stack

- C# / .NET 9
- MSTest for unit testing
- JSON For Saving/Loading Serialization

## Course

CS 3500 — Software Practice, University of Utah (Fall 2025)  
Solo project across 6 problem sets.
