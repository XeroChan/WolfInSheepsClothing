using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WolfInSheepsClothing
{
    public partial class MainWindow : Window
    {
        private const int NumSheep = 14;
        private readonly Random random = new();
        private readonly List<Sheep> sheeps = new();
        private Wolf? wolf;
        private Barrier? barrier;
        private readonly object sheepLock = new();
        private readonly object wolfLock = new();

        public MainWindow()
        {
            InitializeComponent();
        }

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
                    lock (wolfLock)
                    {
                        wolf?.Move(sheeps);
                    }
                    lock (sheepLock)
                    {
                        foreach (var sheep in sheeps)
                        {
                            sheep.Move(wolf);
                        }
                    }
                });
            });

            foreach (var sheep in sheeps)
            {
                Thread sheepThread = new Thread(() => SheepThreadWork(sheep));
                sheepThread.Start();
            }

            if (wolf != null)
            {
                Thread wolfThread = new Thread(WolfThreadWork);
                wolfThread.Start();
            }
        }

        private void SheepThreadWork(Sheep sheep)
        {
            while (true)
            {
                lock (sheepLock)
                {
                    sheep.Move(wolf);
                }
                barrier?.SignalAndWait();
                Thread.Sleep(100);
            }
        }

        private void WolfThreadWork()
        {
            while (true)
            {
                lock (wolfLock)
                {
                    wolf?.Move(sheeps);
                }
                barrier?.SignalAndWait();
                Thread.Sleep(100);
            }
        }
    }
}
