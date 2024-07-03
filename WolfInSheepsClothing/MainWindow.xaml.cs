using System.Windows; 

namespace WolfInSheepsClothing //Newest commit - backing up
{
    public partial class MainWindow : Window
    {
        private const int NumSheep = 14;
        private readonly Random random = new Random();
        private readonly List<Sheep> sheeps = new List<Sheep>();
        private Wolf wolf;
        private Barrier barrier;
        private readonly object wolfLock = new object();
        private readonly object groupSheepLock = new object();

        public MainWindow()
        {
            InitializeComponent();
        }

        public void DisplayEndScreen()
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

            barrier = new Barrier(NumSheep + 1, b =>
            {
                lock (wolfLock)
                {
                    wolf.Move(sheeps);
                }

                lock (groupSheepLock)
                {
                    foreach (var sheep in sheeps)
                    {
                        sheep.Move(wolf);
                    }
                }
            });

            for (int i = 0; i < NumSheep; i++)
            {
                Sheep sheep = new Sheep(MyCanvas, random);
                sheeps.Add(sheep);
            }

            wolf = new Wolf(MyCanvas, random, this);

            foreach (var sheep in sheeps)
            {
                Thread sheepThread = new Thread(() => SheepThreadWork(sheep));
                sheepThread.Start();
            }

            Thread wolfThread = new Thread(WolfThreadWork);
            wolfThread.Start();
        }

        private void SheepThreadWork(Sheep sheep)
        {
            while (true)
            {
                sheep.Move(wolf);
                barrier.SignalAndWait();
                Thread.Sleep(100);
            }
        }

        private void WolfThreadWork()
        {
            while (true)
            {
                lock (wolfLock)
                {
                    wolf.Move(sheeps);
                }
                barrier.SignalAndWait();
                Thread.Sleep(100);
            }
        }
    }
}

