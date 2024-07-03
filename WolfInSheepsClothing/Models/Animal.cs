using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WolfInSheepsClothing
{
    public abstract class Animal
    {
        internal Image image;
        protected Canvas canvas;
        protected Random random;
        internal int x;
        internal int y;

        public Animal(Canvas canvas, Random random, string imagePath)
        {
            this.canvas = canvas;
            this.random = random;
            x = random.Next(0, (int)canvas.ActualWidth);
            y = random.Next(0, (int)canvas.ActualHeight);
            image = new Image
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Width = 40,
                Height = 40
            };
            Canvas.SetLeft(image, x);
            Canvas.SetTop(image, y);
            canvas.Children.Add(image);
        }

        protected void UpdatePosition()
        {
            canvas.Dispatcher.Invoke(() =>
            {
                Canvas.SetLeft(image, x);
                Canvas.SetTop(image, y);
            });
        }
    }
}
