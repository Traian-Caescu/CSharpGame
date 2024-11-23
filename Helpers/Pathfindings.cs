using System;
using System.Collections.Generic;
using WpfApp1.Models;

namespace WpfApp1.Helpers
{
    public static class Pathfinding
    {
        public static List<(int x, int y)> FindPathAvoidingPolice(bool[,] walls, int startX, int startY, int endX, int endY, List<Police> policeUnits)
        {
            // queue for nodes to explore (BFS)
            var openSet = new Queue<(int x, int y)>();
            // dictionary used to reconstruct the path once the end is reached
            var cameFrom = new Dictionary<(int, int), (int, int)>();
            // avoid revisiting
            var visited = new HashSet<(int, int)>();

            openSet.Enqueue((startX, startY));
            visited.Add((startX, startY));

            //BFS loop
            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                // if end position, reconstruct and return the path
                if (current == (endX, endY))
                {
                    return ReconstructPath(cameFrom, current);
                }

                // explore neighbours of the current position
                foreach (var neighbor in GetNeighbors(walls, current.x, current.y, policeUnits))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        openSet.Enqueue(neighbor); // add neighbor to queue
                        cameFrom[neighbor] = current;
                    }
                }
            }

            return new List<(int, int)>(); // Return an empty path if no path is found
        }

        // get valid neighboring cells that enemy can move to
        private static List<(int x, int y)> GetNeighbors(bool[,] walls, int x, int y, List<Police> policeUnits)
        {
            var neighbors = new List<(int x, int y)>
            {
                (x - 1, y), 
                (x + 1, y), 
                (x, y - 1), 
                (x, y + 1)  
            };

            // removes neighbors that are out of bounce
            neighbors.RemoveAll(n =>
                n.x < 0 || n.y < 0 ||
                n.x >= walls.GetLength(1) ||
                n.y >= walls.GetLength(0) ||
                walls[n.y, n.x] || // Wall check
                IsOccupiedByPolice(n, policeUnits)); // Police check

            return neighbors;
        }

        private static bool IsOccupiedByPolice((int x, int y) position, List<Police> policeUnits)
        {
            foreach (var police in policeUnits)
            {
                if (police.X == position.x && police.Y == position.y)
                {
                    return true;
                }
            }
            return false; // not occupied by police
        }

        // reconstruct the path from start to the end using cameFrom map
        // cameFrom - dictionary that maps each cell to its previous cell in the path

        private static List<(int x, int y)> ReconstructPath(Dictionary<(int, int), (int, int)> cameFrom, (int x, int y) current)
        {
            var path = new List<(int, int)> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }
    }
}
