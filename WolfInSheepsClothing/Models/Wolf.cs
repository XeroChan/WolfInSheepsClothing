using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace WolfInSheepsClothing
{
    public class Wolf : Animal
    {
        private const int MoveStepWolf = 8;
        private MainWindow mainWindow;

        public Wolf(Canvas canvas, Random random, MainWindow mainWindow)
            : base(canvas, random, "/Images/wolf.png")
        {
            this.mainWindow = mainWindow;
        }

        public void Move(List<Sheep> sheeps)
        {
            if (sheeps.Count == 0)
            {
                canvas.Dispatcher.Invoke(() =>
                {
                    mainWindow.DisplayEndScreen();
                });
                return;
            }

            var nearestSheep = sheeps.OrderBy(s => Math.Sqrt(Math.Pow(s.x - x, 2) + Math.Pow(s.y - y, 2))).First();

            if (Math.Sqrt(Math.Pow(nearestSheep.x - x, 2) + Math.Pow(nearestSheep.y - y, 2)) <= 4 * MoveStepWolf)
            {
                // Jump to the sheep and eat it
                x = nearestSheep.x;
                y = nearestSheep.y;
                canvas.Dispatcher.Invoke(() =>
                {
                    canvas.Children.Remove(nearestSheep.image);
                });
                sheeps.Remove(nearestSheep);
            }
            else
            {
                // Move towards the nearest sheep
                x += Math.Sign(nearestSheep.x - x) * MoveStepWolf;
                y += Math.Sign(nearestSheep.y - y) * MoveStepWolf;

                x = Math.Max(0, Math.Min((int)canvas.ActualWidth - (int)image.Width, x));
                y = Math.Max(0, Math.Min((int)canvas.ActualHeight - (int)image.Height, y));
                UpdatePosition();
            }
        }
    }
}
