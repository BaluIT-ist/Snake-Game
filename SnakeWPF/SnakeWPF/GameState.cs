﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SnakeWPF
{
    public class GameState
    {
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private AppleEnum Apples { get; set; }
        public FoodStateEnum foodState { get; set; }

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();

        private readonly LinkedList<Position> snakePosition = new LinkedList<Position>();
        private readonly Random random = new Random();

        public GameState(int rows, int cols, AppleEnum randomFood)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right;
            Apples = randomFood;

            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int r = Rows / 2;
            for (int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePosition.AddFirst(new Position(r, c));
            }
        }


        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }



        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());
            if (empty.Count == 0)
            {
                return;
            }

            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
        }

        public Position HeadPosition()
        {
            return snakePosition.First.Value;
        }
        public Position TailPosition()
        {
            return snakePosition.Last.Value;
        }

        public IEnumerable<Position> SnakePosition()
        {
            return snakePosition;
        }

        private void AddHead(Position pos)
        {
            snakePosition.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = snakePosition.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            snakePosition.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Dir;
            }
            return dirChanges.Last.Value;
        }

        public bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false;
            }
            Direction LastDir = GetLastDirection();
            return newDir != LastDir && newDir != LastDir.Oposite();
        }
        public void ChangeDirection(Direction dir)
        {
            if (CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
        }

        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }
        private GridValue WillHit(Position newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }

            if (newHeadPos == TailPosition())

            {
                return GridValue.Empty;
            }

            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public async void Move()
        {
            foodState = FoodStateEnum.NO_NEW_FOOD;

            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveLast();
            }
            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);


            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if (hit == GridValue.Food)
            {
                AddHead(newHeadPos);
                AddScore();
                await Task.Delay(10);
                AddFood();

                foodState = FoodStateEnum.ADD_FOOD;
            }

            foodState = FoodStateEnum.NO_NEW_FOOD;

        }

        private void AddScore()
        {
            switch (Apples)
            {
                case AppleEnum.RED_APPLE:
                    Score++;
                    break;
                case AppleEnum.GREEN_APPLE:
                    Score = Score + 10;
                    break;
                case AppleEnum.PINKDIAMOND_APPLE:
                    Score = Score + 25;
                    break;
                case AppleEnum.GOLDEN_APPLE:
                    Score = Score + 100;
                    break;
            }

        }



    }
}




