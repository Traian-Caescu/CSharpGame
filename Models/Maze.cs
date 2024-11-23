using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfApp1.Models
{
    public class Maze
    {
        //2D array representing walls. true = wall
        public bool[,] Walls { get; private set; }
        private const int Rows = 15;
        private const int Columns = 20;
        private List<Image> wallImages = new List<Image>();
        private const int CellSize = 50;

        public Maze()
        {
            Walls = new bool[Rows, Columns];
            GenerateGridBasedMaze(); // populate walls array with maze structure
        }

        public bool IsValidMove(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Columns && y < Rows && !Walls[y, x];
        }

        // generat a grid-based maze using recursive back tracking
        // walls are initialized as solid and a random path is carved
        private void GenerateGridBasedMaze()
        {
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    Walls[y, x] = true;
                }
            }

            Random random = new Random();
            Stack<(int x, int y)> pathStack = new Stack<(int x, int y)>();
            int startX = 0, startY = 0;
            // start maze generation from top left corner
            pathStack.Push((startX, startY));
            Walls[startY, startX] = false; // mark starting cell as open

            //recursive backtracking to carve paths
            while (pathStack.Count > 0)
            {
                var (x, y) = pathStack.Pop();
                var neighbors = GetValidNeighbors(x, y);

                if (neighbors.Count > 0)
                {
                    pathStack.Push((x, y)); // re-add current cells to stack

                    // randomly select a neighboring cell and carve a path
                    var (nx, ny) = neighbors[random.Next(neighbors.Count)];
                    Walls[(y + ny) / 2, (x + nx) / 2] = false;
                    Walls[ny, nx] = false; // open neighbor cell

                    pathStack.Push((nx, ny));
                }
            }
            // clean areas around the start and endpoints to ensure they are accessible
            ClearSurroundingCells(0, 0);
            ClearSurroundingCells(Rows - 1, Columns - 1);
        }
        // retrive valid neighboring cells for carving paths
        private List<(int x, int y)> GetValidNeighbors(int x, int y)
        {
            var neighbors = new List<(int x, int y)>
            {
                (x - 2, y), 
                (x + 2, y), 
                (x, y - 2), 
                (x, y + 2)  
            };
            // filter out neighbors out of bounds or already opened
            neighbors.RemoveAll(n => !IsInBounds(n.x, n.y) || !Walls[n.y, n.x]);
            return neighbors;
        }

        // coordinates are within maze bounds
        private bool IsInBounds(int x, int y) => x >= 0 && y >= 0 && x < Columns && y < Rows;

        // make area ccessible ( start and end points )
        private void ClearSurroundingCells(int x, int y)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (IsInBounds(nx, ny))
                    {
                        Walls[ny, nx] = false;
                    }
                }
            }
        }

        public (int x, int y) GetRandomEmptyCell()
        {
            Random random = new Random();
            int x, y;
            do
            {
                x = random.Next(Columns);
                y = random.Next(Rows);
            } while (Walls[y, x]); // retry if wall

            return (x, y);
        }

        // clears a direct path from start to end
        public void ClearPath(int startX, int startY, int endX, int endY)
        {
            int currentX = startX;
            int currentY = startY;

            while (currentX != endX || currentY != endY)
            {
                Walls[currentY, currentX] = false;

                if (currentX < endX) currentX++;
                else if (currentX > endX) currentX--;

                if (currentY < endY) currentY++;
                else if (currentY > endY) currentY--;
            }

            Walls[endY, endX] = false; // Ensure the destination cell is open
        }
        // place wall images in cells
        public void RenderMaze(Canvas canvas)
        {
            //clear any previous wall images from the canvas
            wallImages.ForEach(wall => canvas.Children.Remove(wall));
            wallImages.Clear();

            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    if (Walls[y, x])
                    {
                        Image wall = new Image
                        {
                            Width = CellSize,
                            Height = CellSize,
                            Source = new BitmapImage(new Uri("pack://application:,,,/Images/Wall.jpg"))
                        };
                        canvas.Children.Add(wall);
                        Canvas.SetLeft(wall, x * CellSize);
                        Canvas.SetTop(wall, y * CellSize);
                        wallImages.Add(wall);
                    }
                }
            }
        }
    }
}
