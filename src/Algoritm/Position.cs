using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PokemonSolver.Debug;
using PokemonSolver.Memory;

namespace PokemonSolver.Algoritm
{
    public class Position : IShortStringable
    {
        private static MapData.MapData? _mapData = null;

        public uint MapBank { get; }
        public uint MapIndex { get; }
        public uint X { get; }
        public uint Y { get; }
        public Direction Direction { get; }
        public Altitude Altitude { get; }

        public Position(uint bank, uint index, uint x, uint y, Direction direction, Altitude altitude)
        {
            MapBank = bank;
            MapIndex = index;
            X = x;
            Y = y;
            Direction = direction;
            Altitude = altitude;
        }

        public float MinimumDistance(Position p)
        {
            if (MapBank != p.MapBank || MapIndex != p.MapIndex)
                return 10000; //FIXME
            var distX = X > p.X ? X - p.X : p.X - X;
            var distY = Y > p.Y ? Y - p.Y : p.Y - Y;
            var dirPenalty = Direction == p.Direction ? 0 : 1;

            return distX + distY + dirPenalty;
        }

        public List<Position> Neighbours()
        {
            var neighbours = new List<Position>(4);
            // var currentTile = _mapData.GetTile(X,Y);
            foreach (var d in (Direction[])Enum.GetValues(typeof(Direction)))
            {
                if (Direction != d)
                {
                    neighbours.Add(new Position(MapBank, MapIndex, X, Y, d, Altitude));
                }
                else
                {
                    var pos = Forward();
                    if (pos != null)
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
            if (_mapData == null)
                throw new Exception("Attempting to compute Forward Position without having called SetMapData in Position.cs");
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

            if (x >= _mapData.Width || y >= _mapData.Height) // this actually handles -1 as well as it's an uint and underflows back to uint.maxvalue
                return null;

            if (!CanMoveTo(x, y))
                return null;

            return new Position(MapBank, MapIndex, x, y, Direction, Altitude); //TODO check altitude change
        }

        public bool CanMoveTo(uint x, uint y)
        {
            //check with Route 110 && route 114
            if (_mapData == null)
                throw new Exception("Attempting to compute CanMoveTo without having called SetMapData in Position.cs");

            var from = _mapData.GetTile(X, Y);
            var to = _mapData.GetTile(x, y);
            if (!to.Walkable)
                return false;

            if (from.MovementPermission == to.MovementPermission)
                return true;
            // X -> 0 -> X
            if (from.MovementPermission == 0 || to.MovementPermission == 0)
                return true;

            return false;
            // C -> 3C -> C if alt low
            // Tuple<Altitude, byte, byte> allowedChange;
            // if (altitude == Altitude.Ground)
            // {
            //     if (from.MovementPermission == 0xC && MovementPermission == 0x3C)
            //         return true;
            //     if (from.MovementPermission == 0x3C && MovementPermission == 0xC)
            //         return true;
            // }
            // 10 -> 3C -> 10 if alt high
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

        public static void SetMapData(MapData.MapData mapData)
        {
            _mapData = mapData;
        }
    }
}