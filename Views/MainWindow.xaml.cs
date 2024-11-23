using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WpfApp1.Models;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using WpfApp1.Helpers;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        // Timers to control variaus aspects of the game
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private DispatcherTimer enemyTimer = new DispatcherTimer();
        private DispatcherTimer policePatrolTimer = new DispatcherTimer();
        private DispatcherTimer aggressionTimer = new DispatcherTimer(); // Timer for aggression duration
        //Game entities
        private Player player;
        private Maze maze;
        private List<Enemy> enemies = new List<Enemy>();
        private List<Police> policeUnits = new List<Police>();
        private Image exit;
        //Game state variablels 
        private int exitX, exitY;
        private int cellSize = 50;
        private int health = 3;
        private bool enemyIsAggressive = false;
        private (int x, int y) lastKnownEnemyPosition;
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            
            maze = new Maze();
            maze.RenderMaze(GameCanvas);
            SetupGame();

            gameTimer.Interval = TimeSpan.FromMilliseconds(200);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            enemyTimer.Interval = TimeSpan.FromMilliseconds(1000);
            enemyTimer.Tick += MoveEnemies;
            enemyTimer.Start();

            policePatrolTimer.Interval = TimeSpan.FromMilliseconds(1000);
            policePatrolTimer.Tick += PatrolPolice;
            policePatrolTimer.Start();

            aggressionTimer.Interval = TimeSpan.FromSeconds(5); // Set aggression duration to 5 seconds
            aggressionTimer.Tick += EndAggression; 

            //Key press event for player movement
            this.KeyDown += OnKeyDown;
        }

        // initial game state with player, enemies, police and exit positions
        private void SetupGame()
        {
            var (playerX, playerY) = maze.GetRandomEmptyCell();
            player = new Player();
            GameCanvas.Children.Add(player.Sprite);
            player.MoveTo(GameCanvas, playerX, playerY);

            (exitX, exitY) = maze.GetRandomEmptyCell();
            exit = new Image
            {
                Width = cellSize,
                Height = cellSize,
                Source = new BitmapImage(new Uri("pack://application:,,,/Images/Exit.jpg"))
            };
            GameCanvas.Children.Add(exit);
            Canvas.SetLeft(exit, exitX * cellSize);
            Canvas.SetTop(exit, exitY * cellSize);

            var (enemyX, enemyY) = maze.GetRandomEmptyCell();
            AddEnemy(enemyX, enemyY);

            var (policeX, policeY) = maze.GetRandomEmptyCell();
            AddPoliceUnit(policeX, policeY);

            //ensure the valid path exists
            EnsurePathExists();
        }

        private void EnsurePathExists()
        {
            var path = Pathfinding.FindPathAvoidingPolice(maze.Walls, player.X, player.Y, exitX, exitY, policeUnits);

            if (path.Count == 0)
            {
                maze.ClearPath(player.X, player.Y, exitX, exitY);
                maze.RenderMaze(GameCanvas);
            }
        }

        private void AddEnemy(int startX, int startY)
        {
            var enemy = new Enemy();
            GameCanvas.Children.Add(enemy.Sprite);
            enemy.MoveTo(GameCanvas, startX, startY);
            enemies.Add(enemy);
        }

        private void AddPoliceUnit(int startX, int startY)
        {
            var police = new Police();
            GameCanvas.Children.Add(police.Sprite);
            police.MoveTo(GameCanvas, startX, startY);
            policeUnits.Add(police);
        }

        //Main loop, checks for collisions and ensures path validity
        private void GameLoop(object sender, EventArgs e)
        {
            CheckCollisions();
            EnsurePathExists();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            int newX = player.X;
            int newY = player.Y;

            if (e.Key == Key.Up) newY--;
            else if (e.Key == Key.Down) newY++;
            else if (e.Key == Key.Left) newX--;
            else if (e.Key == Key.Right) newX++;

            if (maze.IsValidMove(newX, newY))
            {
                player.MoveTo(GameCanvas, newX, newY);
            }
        }

        //controls enemy movement and otgles aggression behaviour
        private void MoveEnemies(object sender, EventArgs e)
        {
            ToggleEnemyBehavior();

            foreach (var enemy in enemies)
            {
                (int x, int y) = enemy.GetNextPosition(player.X, player.Y, maze.Walls, policeUnits);

                if (maze.IsValidMove(x, y) && !(x == exitX && y == exitY))
                {
                    enemy.MoveTo(GameCanvas, x, y);
                    lastKnownEnemyPosition = (x, y); // Update last known enemy position when moving
                }
            }
        }


        private void ToggleEnemyBehavior()
        {
            if (!enemyIsAggressive)
            {
                enemyIsAggressive = true;
                enemyTimer.Interval = TimeSpan.FromMilliseconds(700); // Faster speed for aggressive mode
                aggressionTimer.Start(); // Start the aggression duration timer
            }
        }

        private void EndAggression(object sender, EventArgs e)
        {
            enemyIsAggressive = false;
            enemyTimer.Interval = TimeSpan.FromMilliseconds(1000); // Reset to normal speed
            aggressionTimer.Stop(); // Stop the aggression timer
        }
        
        //control police behavior
        private void PatrolPolice(object sender, EventArgs e)
        {
            if (enemyIsAggressive) 
            {
                foreach (var police in policeUnits)
                {
                    (int x, int y) = police.GetNextPositionTowards(lastKnownEnemyPosition.x, lastKnownEnemyPosition.y, maze.Walls);

                    if (maze.IsValidMove(x, y))
                    {
                        police.MoveTo(GameCanvas, x, y);
                    }
                }
            }
        }

        private void CheckCollisions()
        {
            // Check player and enemy collisions
            foreach (var enemy in enemies.ToArray()) // Use ToArray to avoid modifying the collection during iteration
            {
                if (enemy.IsColliding(player))
                {
                    health--;
                    if (health <= 0)
                    {
                        GameOver("You were caught by an enemy and lost all health!");
                    }
                }

                // Check enemy and police collisions
                foreach (var police in policeUnits)
                {
                    if (police.IsColliding(enemy))
                    {
                        GameCanvas.Children.Remove(enemy.Sprite); // Remove enemy sprite from the canvas
                        enemies.Remove(enemy); // Remove enemy from the list
                        break; 
                    }
                }
            }

            // Check player and exit collision
            if (player.X == exitX && player.Y == exitY)
            {
                GameOver("You reached the exit and won!");
            }
        }


        private void GameOver(string message)
        {
            gameTimer.Stop();
            enemyTimer.Stop();
            policePatrolTimer.Stop();
            aggressionTimer.Stop();
            MessageBox.Show(message);
            Application.Current.Shutdown();
        }
    }
}
