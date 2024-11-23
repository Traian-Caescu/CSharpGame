using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfApp1.Models
{
    public class Police
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Image Sprite { get; private set; }

        //constructor - image
        public Police()
        {
            Sprite = new Image
            {
                Width = 50,
                Height = 50,
                Source = new BitmapImage(new Uri("pack://application:,,,/Images/Police.jpg"))
            };
        }

        public void MoveTo(Canvas canvas, int x, int y)
        {
            X = x;
            Y = y;
            Canvas.SetLeft(Sprite, x * 50);
            Canvas.SetTop(Sprite, y * 50);
        }

        // Determine next position to move towards target
        public (int, int) GetNextPositionTowards(int targetX, int targetY, bool[,] walls)
        {
            // calculate movement direction on both axes
            int deltaX = targetX > X ? 1 : targetX < X ? -1 : 0;
            int deltaY = targetY > Y ? 1 : targetY < Y ? -1 : 0;

            // calculate next potential position
            int nextX = X + deltaX;
            int nextY = Y + deltaY;

            if (IsValidMove(nextX, nextY, walls))
            {
                return (nextX, nextY);
            }

            // If direct path is blocked, attempt adjacent cells
            if (deltaX != 0 && IsValidMove(X + deltaX, Y, walls))
            {
                return (X + deltaX, Y);
            }
            if (deltaY != 0 && IsValidMove(X, Y + deltaY, walls))
            {
                return (X, Y + deltaY);
            }

            return (X, Y); // Stay in place if no valid move found
        }
        // random patrol position around current. 
        public (int, int) GetRandomPatrolPosition(bool[,] walls)
        {
            Random random = new Random();
            int newX, newY;
            do
            {
                // randomly select adjacent cell
                newX = X + random.Next(-1, 2); // -1, 0, or +1
                newY = Y + random.Next(-1, 2); // -1, 0, or +1
            } while (!IsValidMove(newX, newY, walls) || (newX == X && newY == Y));

            return (newX, newY);
        }

        private bool IsValidMove(int x, int y, bool[,] walls)
        {
            return x >= 0 && y >= 0 && x < walls.GetLength(1) && y < walls.GetLength(0) && !walls[y, x];
        }

        public bool IsColliding(Enemy enemy)
        {
            return X == enemy.X && Y == enemy.Y;
        }

    }
}
