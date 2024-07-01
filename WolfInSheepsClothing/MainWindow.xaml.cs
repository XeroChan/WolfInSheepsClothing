using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WolfInSheepsClothing
{
    public partial class MainWindow : Window
    {
        private const int NumSheep = 14;
        private const int MoveStepSheep = 9;
        private const int MoveStepWolf = 8;
        private readonly Random random = new();
        private readonly List<Sheep> sheeps = new();
        private Wolf? wolf;
        private Barrier? barrier;
        private DispatcherTimer sheepTimer;
        private DispatcherTimer wolfTimer;

        public MainWindow() => InitializeComponent();

        private void DisplayEndScreen()
        {
            MyCanvas.Children.Clear();

            EndText.Text = "Game Over! All sheep have been caught by the wolf.";
            EndText.Visibility = Visibility.Visible;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartBtn.Visibility = Visibility.Collapsed;
            MyCanvas.Children.Clear();
            sheeps.Clear();

            for (int i = 0; i < NumSheep; i++)
            {
                Sheep sheep = new(MyCanvas, random);
                sheeps.Add(sheep);
            }

            wolf = new Wolf(MyCanvas, random, this);

            barrier = new Barrier(NumSheep + 1, (b) =>
            {
                Dispatcher.Invoke(() =>
                {
                    wolf.Move(sheeps);
                    foreach (var sheep in sheeps)
                    {
                        sheep.Move(wolf);
                    }
                });
            });

            // Setup timers
            sheepTimer = new DispatcherTimer();
            sheepTimer.Tick += SheepTimer_Tick;
            sheepTimer.Interval = TimeSpan.FromMilliseconds(100);

            wolfTimer = new DispatcherTimer();
            wolfTimer.Tick += WolfTimer_Tick;
            wolfTimer.Interval = TimeSpan.FromMilliseconds(100);

            
            sheepTimer.Start();
            wolfTimer.Start();
        }

        private void SheepTimer_Tick(object sender, EventArgs e)
        {
            foreach (var sheep in sheeps)
            {
                sheep.Move(wolf);
            }
        }

        private void WolfTimer_Tick(object sender, EventArgs e)
        {
            wolf.Move(sheeps);
        }

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
                Canvas.SetLeft(image, x);
                Canvas.SetTop(image, y);
            }
        }

        public class Sheep : Animal
        {
            public Sheep(Canvas canvas, Random random) : base(canvas, random, "/Images/sheep.png")
            {
            }

            public void Move(Wolf wolf)
            {
                double distanceToWolf = Math.Sqrt(Math.Pow(wolf.x - x, 2) + Math.Pow(wolf.y - y, 2));

                if (distanceToWolf <= 8 * MoveStepSheep)
                {
                    Flee(wolf);
                }
                else
                {
                    // Random movement
                    x += random.Next(-MoveStepSheep, MoveStepSheep);
                    y += random.Next(-MoveStepSheep, MoveStepSheep);

                    // Prevent sheep from getting stuck in the corners
                    if (x <= 0) x += MoveStepSheep;
                    if (y <= 0) y += MoveStepSheep;
                    if (x >= canvas.ActualWidth - image.Width) x -= MoveStepSheep;
                    if (y >= canvas.ActualHeight - image.Height) y -= MoveStepSheep;

                    x = Math.Max(0, Math.Min((int)canvas.ActualWidth - (int)image.Width, x));
                    y = Math.Max(0, Math.Min((int)canvas.ActualHeight - (int)image.Height, y));
                    UpdatePosition();
                }
            }

            private void Flee(Wolf wolf)
            {
                int dx = x - wolf.x;
                int dy = y - wolf.y;

                // Adjust the sheep's position based on the direction to flee
                x += dx > 0 ? MoveStepSheep : -MoveStepSheep;
                y += dy > 0 ? MoveStepSheep : -MoveStepSheep;

                // Prevent sheep from getting stuck in the corners
                if (x <= 0) x += MoveStepSheep;
                if (y <= 0) y += MoveStepSheep;
                if (x >= canvas.ActualWidth - image.Width) x -= MoveStepSheep;
                if (y >= canvas.ActualHeight - image.Height) y -= MoveStepSheep;

                x = Math.Max(0, Math.Min((int)canvas.ActualWidth - (int)image.Width, x));
                y = Math.Max(0, Math.Min((int)canvas.ActualHeight - (int)image.Height, y));
                UpdatePosition();
            }

            public static void Run(object barrierObj)
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

            public void Move()
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

                var nearestSheep = sheeps.OrderBy(s => Math.Sqrt(Math.Pow(s.x - x, 2) + Math.Pow(s.y - y, 2))).First();

                if (Math.Sqrt(Math.Pow(nearestSheep.x - x, 2) + Math.Pow(nearestSheep.y - y, 2)) <= 4 * MoveStepWolf)
                {
                    // Jump to the sheep and eat it
                    x = nearestSheep.x;
                    y = nearestSheep.y;
                    canvas.Children.Remove(nearestSheep.image);
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


            public static void Run(object barrierObj)
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