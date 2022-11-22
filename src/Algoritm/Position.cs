using System;
using System.Collections.Generic;
using PokemonSolver.Debug;
using PokemonSolver.Memory;

namespace PokemonSolver.Algoritm
{
    public class Position: IShortStringable
    {
        public uint X { get; }
        public uint Y { get; }
        public Direction Direction { get; }

        public Position(uint x, uint y, Direction direction)
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
                    var pos = Forward();
                    if(pos != null)
                        neighbours.Add(pos);
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Returns a copy with one step forward
        /// </summary>
        /// <returns></returns>
        public Position? Forward()
        {
            uint x = X, y = Y;
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
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (x == uint.MaxValue || y == uint.MaxValue)
            {
                return null;
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

        public string ToShortString()
        {
            return $"{X},{Y},{Direction.ToString()[0]}";
        }

        public override string ToString()
        {
            return $"Position({X},{Y},{Direction})";
        }
    }
}