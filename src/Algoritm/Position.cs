using System;
using System.Collections.Generic;

namespace PokemonSolver.Algoritm
{
    public class Position
    {
        public int X { get; }
        public int Y { get; }
        public Direction Direction { get; }

        public Position(int x, int y, Direction direction)
        {
            X = x;
            Y = y;
            Direction = direction;
        }

        public float MinimumDistance(Position p)
        {
            //TODO include direction
            return Math.Abs(X - p.X) + Math.Abs(Y - p.Y);
        }

        public List<Position> Neighbours()
        {
            var neighbours = new List<Position>(4);

            foreach (var d in (Direction[])Enum.GetValues(typeof(Direction)))
            {
                if (Direction != d)
                {
                    neighbours.Add(new Position(X, Y, d));
                }
                else
                {
                    neighbours.Add(Forward());
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Returns a copy with one step forward
        /// </summary>
        /// <returns></returns>
        public Position Forward()
        {
            int x = X, y = Y;
            switch (Direction)
            {
                case Direction.Left:
                    x--;
                    break;
                case Direction.Right:
                    x++;
                    break;
                case Direction.Up:
                    y--;
                    break;
                case Direction.Down:
                    y++;
                    break;
            }

            return new Position(x, y, Direction);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Position))
                return false;

            var p = (Position)obj;
            return X == p.X && Y == p.Y && Direction == p.Direction;
        }

        public override string ToString()
        {
            return $"Position({X},{Y},{Direction})";
        }
    }
}