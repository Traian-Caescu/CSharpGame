using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WpfApp1.Models
{
    public class Player
    {
        public Image Sprite { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        private const int CellSize = 50;

        public Player()
        {
            Sprite = new Image
            {
                Width = CellSize,
                Height = CellSize,
                Source = new BitmapImage(new Uri("pack://application:,,,/WpfApp1;component/Images/Player.jpg"))
            };
        }
        
        public void MoveTo(Canvas canvas, int x, int y)
        {
            Canvas.SetLeft(Sprite, x * CellSize);
            Canvas.SetTop(Sprite, y * CellSize);
            X = x;
            Y = y;
        }

    }
}
