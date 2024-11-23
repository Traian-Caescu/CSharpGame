using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using WpfApp1.Helpers;

namespace WpfApp1.Models
{
    public class Enemy
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Image Sprite { get; private set; }

        public Enemy()
        {
            Sprite = new Image
            {
                Width = 50,
                Height = 50,
                Source = new BitmapImage(new Uri("pack://application:,,,/Images/Enemy.jpg"))
            };
        }

        public void MoveTo(Canvas canvas, int x, int y)
        {
            X = x;
            Y = y;
            Canvas.SetLeft(Sprite, x * 50);
            Canvas.SetTop(Sprite, y * 50);
        }
        //determine the next position for the enemy to move towards the player while avoiding police
        public (int, int) GetNextPosition(int playerX, int playerY, bool[,] walls, List<Police> policeUnits)
        {
            var path = Pathfinding.FindPathAvoidingPolice(walls, X, Y, playerX, playerY, policeUnits);

            if (path.Count > 1)
            {
                return path[1];
            }

            return (X, Y); // Stay in place if no path is found
        }

        // check if enemy collided with the player
        public bool IsColliding(Player player)
        {
            return X == player.X && Y == player.Y;
        }
    }
}
