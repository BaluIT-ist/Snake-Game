using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SnakeWPF
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource> gridValToImage = new Dictionary<GridValue, ImageSource>();

        private readonly int rows = 20, cols = 20;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private GridValue gridVal;
        private bool gameRunning;
        private readonly Dictionary<Direction, int> dirToRotation = new Dictionary<Direction, int>()
        {
            {Direction.Up ,    0 },
            {Direction.Right ,90 },
            {Direction.Down ,180 },
            {Direction.Left, 270 },
        };


        public MainWindow()
        {
            AppleEnum randomFood = FoodRandomDrop();
            gridValToImage.Add(GridValue.Empty, Images.Empty);
            gridValToImage.Add(GridValue.Snake, Images.Body);
            // gridValToImage.Add(GridValue.Food, Images.Food);
            switch (randomFood)
            {
                case AppleEnum.RED_APPLE:
                    gridValToImage.Add(GridValue.Food, Images.RedApple);
                    break;
                case AppleEnum.GREEN_APPLE:
                    gridValToImage.Add(GridValue.Food, Images.GreenApple);
                    break;
                case AppleEnum.PINKDIAMOND_APPLE:
                    gridValToImage.Add(GridValue.Food, Images.PinkDiamongApple);
                    break;
                case AppleEnum.GOLDEN_APPLE:
                    gridValToImage.Add(GridValue.Food, Images.GoldenApple);
                    break;
            }



            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols, randomFood);
        }



        private async Task RunGame()
        {
            AppleEnum randomFood = FoodRandomDrop();
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameState = new GameState(rows, cols, randomFood);
        }

        private async void Window_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!gameRunning)
            {
                gameRunning = true;
                await RunGame();
                gameRunning = false;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.Left:
                    gameState.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                    gameState.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                    gameState.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    gameState.ChangeDirection(Direction.Down);
                    break;
            }
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                await Task.Delay(100);
                gameState.Move();

                if (gameState.foodState == FoodStateEnum.ADD_FOOD)
                {
                    AppleEnum randomFood = FoodRandomDrop();
                    gridValToImage.Remove(GridValue.Food);
                    switch (randomFood)
                    {
                        case AppleEnum.RED_APPLE:
                            gridValToImage.Add(GridValue.Food, Images.RedApple);
                            break;
                        case AppleEnum.GREEN_APPLE:
                            gridValToImage.Add(GridValue.Food, Images.GreenApple);
                            break;
                        case AppleEnum.PINKDIAMOND_APPLE:
                            gridValToImage.Add(GridValue.Food, Images.PinkDiamongApple);
                            break;
                        case AppleEnum.GOLDEN_APPLE:
                            gridValToImage.Add(GridValue.Food, Images.GoldenApple);
                            break;
                    }
                }
                Draw();
            }
        }

        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Image image = new Image()
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };
                    images[r, c] = image;
                    GameGrid.Children.Add(image);


                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            Score_Text.Text = $"SCORE {gameState.Score}";
        }
        private void DrawGrid()
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;


            int rotation = dirToRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }


        private async Task DrawDeadSnake()
        {
            List<Position> positions = new List<Position>(gameState.SnakePosition());
            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(50);
            }
        }
        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "Press any key to restart!";
        }

        /// <summary>
        /// Red Apple has 80% chance to spawn
        /// Green Apple has 10% chance to spawn
        /// Pink Diamond Apple has 7.5% chance to spawn
        /// Golden Apple has 2.5% chance to spawn
        /// </summary>
        /// <returns></returns>
        //private AppleEnum FoodRandomDrop()
        //{

        //    Random random = new Random();
        //    double chance=random.NextDouble();
        //    if(chance < 0.80)
        //    {
        //        return AppleEnum.RED_APPLE;
        //    }
        //    else
        //    {
        //        if (chance < 0.90)
        //        {
        //            return AppleEnum.GREEN_APPLE;
        //        }
        //        else
        //        {
        //            if (chance < 0.975)
        //            {
        //                return AppleEnum.PINKDIAMOND_APPLE;
        //            }
        //            else
        //            {
        //                return AppleEnum.GOLDEN_APPLE;
        //            }
        //        }
        //    }
        //}

        private AppleEnum FoodRandomDrop()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] randomNumber = new byte[4];
                rng.GetBytes(randomNumber);
                int intRandomNumber = BitConverter.ToInt32(randomNumber, 0);
                double chance = (intRandomNumber & 0x7FFFFFFF) / (double)int.MaxValue;
                if (chance < 0.50)
                {
                    return AppleEnum.RED_APPLE;
                }
                else
                {
                    if (chance < 0.70)
                    {
                        return AppleEnum.GREEN_APPLE;
                    }
                    else
                   
                    {
                        if (chance < 0.90)
                        {
                            return AppleEnum.PINKDIAMOND_APPLE;
                        }
                        else
                        {
                            return AppleEnum.GOLDEN_APPLE;
                        }
                    }
                }
            }
        }
    }
}
