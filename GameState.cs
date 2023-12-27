using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    public class GameState
    {
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid {  get; }
        public Direction Dir { get; private set; }
        public int Score {  get; private set; }
        public bool GameOver { get; private set; }
        private readonly LinkedList<Direction> dirChanges   = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        private readonly Random random = new Random();
        public GameState(int nRows, int nCols) { 
            Rows = nRows; 
            Cols = nCols;
            Grid = new GridValue[nRows, nCols];
            Dir = Direction.Right;
            AddSnake();
            AddFood();
        }
        private void AddSnake()
        {
            int r = Rows / 2;
            for(int c=1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));
            }
        }
        private IEnumerable<Position> EmptyPos()
        {
            for(int r= 0; r < Rows; r++)
            {
                for (int c= 0; c < Cols; c++)
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
            List<Position> empty = new List<Position>(EmptyPos());
            if (empty.Count == 0)
            {
                return;
            }
            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row,pos.Col] = GridValue.Food;

        }
        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }
        public Position TailPosition()
        {
            return snakePositions.Last.Value;
        }
        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }
        private void AddHead(Position pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }
        private void RemoveTail()
        {
            Position tail = TailPosition();
            Grid[tail.Row, tail.Col]= GridValue.Empty;
            snakePositions.RemoveLast();
        }
        private Direction GetLastDirection()
        {
            if(dirChanges.Count == 0)
            {
                return Dir;
            }
                return dirChanges.Last.Value;
        }
        private bool CanChangeDirection(Direction newDir)
        {
            if(dirChanges.Count == 2)
            {
                return false;
            }
            Direction lastDir=GetLastDirection();
            return newDir != lastDir && newDir!=lastDir.Opposite();
        }
        public void ChangeDirection(Direction dir)
        {
            //check if can change direction
            if (CanChangeDirection(dir)) {
                dirChanges.AddLast(dir);
            }
        }
        private Boolean isOutside(Position pos)
        {
            return pos.Row<0 || pos.Row>=Rows || pos.Col<0 || pos.Col>=Cols;
        }
        private GridValue isItSelf(Position newHeadPos)
        {
            if (isOutside(newHeadPos))
            {
                return GridValue.Outside;
            }
            //This case the head will hit with the tail and all the snake moves so its not a crash
            if (newHeadPos == TailPosition())
            {
                return GridValue.Empty;
            }
            return Grid[newHeadPos.Row, newHeadPos.Col];
        }
        public void Move()
        {
            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }
                Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = isItSelf(newHeadPos);
            if(hit==GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            } else if( hit==GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }else if(hit == GridValue.Food)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }

        }
    }
}
