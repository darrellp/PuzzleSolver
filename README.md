# PuzzleSolver
This is a backtracking framework with rules.  It works by constructing a solutions tree of Partial solutions.
Each partial solution in the tree has rules applied to it until no more rules apply or until some rule indicates
that it cannot lead to a solution.  Once all rules have been applied and the node hasn't been ruled out by
any of them, the successor nodes in the backtracking tree are created.  Any duplicate nodes are pruned from the
tree and the process repeats until we've found a solution.  The nodes can be ordered according to some heuristic
if desired.  I've used this to both create and solve Battleship type puzzles, sudoku and alphametics.  It uses
very general interfaces so it can be easily adopted to almost any such sort of puzzle.
