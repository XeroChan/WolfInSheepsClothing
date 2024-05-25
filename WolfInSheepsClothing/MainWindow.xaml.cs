using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WolfInSheepsClothing
{
    public partial class MainWindow : Window
    {
        private const int NumSheep = 5;
        private const int MoveStepSheep = 7;
        private const int MoveStepWolf = 8;
        private const int BorderOffset = 20;
        private Random random = new Random();
        private List<Sheep> sheeps = new List<Sheep>();
        private Wolf wolf;
        private Barrier barrier;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void DisplayEndScreen()
        {
            // Clear the canvas
            MyCanvas.Children.Clear();

            // Display the end message
            TextBlock endMessage = new TextBlock
            {
                Text = "Game Over! All sheep have been caught by the wolf.",
                FontSize = 20,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            MyCanvas.Children.Add(endMessage);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartBtn.Visibility = Visibility.Collapsed;
            MyCanvas.Children.Clear();
            sheeps.Clear();

            // Create sheeps
            for (int i = 0; i < NumSheep; i++)
            {
                Sheep sheep = new Sheep(MyCanvas, random);
                sheeps.Add(sheep);
            }

            // Create wolf
            wolf = new Wolf(MyCanvas, random, this);

            // Initialize barrier
            barrier = new Barrier(NumSheep + 1, (b) =>
            {
                // This is the post-phase action. It will be called after all participants signal the barrier.
                Dispatcher.Invoke(() =>
                {
                    foreach (var sheep in sheeps)
                    {
                        sheep.Move();
                    }
                    wolf.Move(sheeps);
                });
            });

            // Start sheep threads
            foreach (var sheep in sheeps)
            {
                ThreadPool.QueueUserWorkItem(sheep.Run, barrier);
            }

            // Start wolf thread
            ThreadPool.QueueUserWorkItem(wolf.Run, barrier);
        }

        public abstract class Animal
        {
            internal Image image;
            protected Canvas canvas;
            protected Random random;
            internal int x;  // Zmieniono z protected na internal
            internal int y;  // Zmieniono z protected na internal

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

            public abstract void Move();

            protected void UpdatePosition()
            {
                Canvas.SetLeft(image, x);
                Canvas.SetTop(image, y);
            }
        }

        public class Sheep : Animal
        {
            public Sheep(Canvas canvas, Random random)
                : base(canvas, random, "/Images/sheep.png")
            {
            }

            public override void Move()
            {
                // Check if the sheep is near the border and adjust the position accordingly
                if ((x - 20) <= BorderOffset) x += (MoveStepSheep + 20);
                else if ((x + 20) >= canvas.ActualWidth - image.Width - BorderOffset) x -= (MoveStepSheep + 20);
                else x += random.Next(-MoveStepSheep, MoveStepSheep);

                if ((y - 20) <= BorderOffset) y += (MoveStepSheep + 20);
                else if ((y + 20) >= canvas.ActualHeight - image.Height - BorderOffset) y -= (MoveStepSheep + 20);
                else y += random.Next(-MoveStepSheep, MoveStepSheep);

                x = Math.Max(0, Math.Min((int)canvas.ActualWidth - (int)image.Width, x));
                y = Math.Max(0, Math.Min((int)canvas.ActualHeight - (int)image.Height, y));
                UpdatePosition();
            }

            public void Flee(Wolf wolf)
            {
                // Calculate the direction to flee from the wolf
                int dx = x - wolf.x;
                int dy = y - wolf.y;

                // Adjust the sheep's position based on the direction to flee
                x += dx > 0 ? MoveStepSheep : -MoveStepSheep;
                y += dy > 0 ? MoveStepSheep : -MoveStepSheep;

                x = Math.Max(0, Math.Min((int)canvas.ActualWidth - (int)image.Width, x));
                y = Math.Max(0, Math.Min((int)canvas.ActualHeight - (int)image.Height, y));
                UpdatePosition();
            }

            public void Run(object barrierObj)
            {
                var barrier = (Barrier)barrierObj;
                while (true)
                {
                    barrier.SignalAndWait();
                    Thread.Sleep(100);
                }
            }
        }


        public class Wolf : Animal
        {
            private MainWindow mainWindow;

            public Wolf(Canvas canvas, Random random, MainWindow mainWindow)
                : base(canvas, random, "/Images/wolf.png")
            {
                this.mainWindow = mainWindow;
            }


            public override void Move()
            {
                x += random.Next(-MoveStepWolf, MoveStepWolf);
                y += random.Next(-MoveStepWolf, MoveStepWolf);
                x = Math.Max(0, Math.Min((int)canvas.ActualWidth - (int)image.Width, x));
                y = Math.Max(0, Math.Min((int)canvas.ActualHeight - (int)image.Height, y));
                UpdatePosition();
            }

            public void Move(List<Sheep> sheeps)
            {
                if (sheeps.Count == 0)
                {
                    mainWindow.DisplayEndScreen();
                    return;
                }
                foreach (var sheep in sheeps)
                {
                    sheep.Flee(this);
                }
                // Chase the nearest sheep
                var nearestSheep = sheeps.OrderBy(s => Math.Sqrt(Math.Pow(s.x - x, 2) + Math.Pow(s.y - y, 2))).First();
                x += Math.Sign(nearestSheep.x - x) * MoveStepWolf;
                y += Math.Sign(nearestSheep.y - y) * MoveStepWolf;

                x = Math.Max(0, Math.Min((int)canvas.ActualWidth - (int)image.Width, x));
                y = Math.Max(0, Math.Min((int)canvas.ActualHeight - (int)image.Height, y));
                UpdatePosition();

                // Check if the wolf has caught the sheep
                if (Math.Sqrt(Math.Pow(nearestSheep.x - x, 2) + Math.Pow(nearestSheep.y - y, 2)) < MoveStepWolf)
                {
                    // Remove the sheep from the canvas and list
                    canvas.Children.Remove(nearestSheep.image);
                    sheeps.Remove(nearestSheep);
                }
            }

            public void Run(object barrierObj)
            {
                var barrier = (Barrier)barrierObj;
                while (true)
                {
                    barrier.SignalAndWait();
                    Thread.Sleep(100);
                }
            }
        }
    }
}
