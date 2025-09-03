using Safari.Persistence.Tiles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence {
    public readonly struct Position {
        public int Row { get; }
        public int Col { get; }

        public Position(int row, int col) {
            Row = row;
            Col = col;
        }
        public Position((int row, int col) cordTuple) {
            Row = cordTuple.row;
            Col = cordTuple.col;
        }

        public override string ToString() => $"({Row},{Col})";
    }

    public class ParallelPathFinder {
        private static readonly int[] rowDirections = { -1, 1, 0, 0 };
        private static readonly int[] colDirections = { 0, 0, -1, 1 };

        public static List<List<Position>> FindAllPaths(Tile[,] matrix, Position start, Position end) {
            if (!IsValid(matrix, start.Row, start.Col) || !IsValid(matrix, end.Row, end.Col))
                return new List<List<Position>>();

            var paths = new ConcurrentBag<List<Position>>();
            var initialVisited = CreateVisitedMatrix(matrix.GetLength(0), matrix.GetLength(1));
            initialVisited[start.Row][start.Col] = true;

            Parallel.For(0, rowDirections.Length, i => {
                int newRow = start.Row + rowDirections[i];
                int newCol = start.Col + colDirections[i];

                if (IsValid(matrix, newRow, newCol) && !initialVisited[newRow][newCol]) {
                    var visited = CloneVisited(initialVisited);
                    visited[newRow][newCol] = true;
                    var path = new List<Position> { start, new Position(newRow, newCol) };
                    ExplorePath(matrix, new Position(newRow, newCol), end, visited, path, paths);
                }
            });

            return paths.ToList();
        }

        private static void ExplorePath(Tile[,] matrix, Position current, Position end,
                                       bool[][] visited, List<Position> path,
                                       ConcurrentBag<List<Position>> paths) {
            if (current.Row == end.Row && current.Col == end.Col) {
                paths.Add(new List<Position>(path));
                return;
            }

            Parallel.For(0, rowDirections.Length, i => {
                int newRow = current.Row + rowDirections[i];
                int newCol = current.Col + colDirections[i];

                if (IsValid(matrix, newRow, newCol) && !visited[newRow][newCol]) {
                    var newVisited = CloneVisited(visited);
                    newVisited[newRow][newCol] = true;
                    var newPath = new List<Position>(path) { new Position(newRow, newCol) };
                    ExplorePath(matrix, new Position(newRow, newCol), end, newVisited, newPath, paths);
                }
            });
        }

        private static bool[][] CreateVisitedMatrix(int rows, int cols) {
            var visited = new bool[rows][];
            for (int i = 0; i < rows; i++)
                visited[i] = new bool[cols];
            return visited;
        }

        private static bool[][] CloneVisited(bool[][] original) {
            return original.Select(a => (bool[])a.Clone()).ToArray();
        }

        private static bool IsValid(Tile[,] matrix, int row, int col) {
            if (row < 0 || row >= matrix.GetLength(0)) return false;
            if (col < 0 || col >= matrix.GetLength(1)) return false;

            return matrix[row, col].GetType().Name == "Road";
        }
    }
}
