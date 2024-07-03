using System.Windows.Controls;

namespace WolfInSheepsClothing
{
    public class Sheep : Animal
    {
        private const int MoveStepSheep = 9;

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
                canvas.Dispatcher.Invoke(() =>
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
                });
            }
        }

        private void Flee(Wolf wolf)
        {
            int dx = x - wolf.x;
            int dy = y - wolf.y;
            canvas.Dispatcher.Invoke(() =>
            {
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
            });
        }
    }
}
