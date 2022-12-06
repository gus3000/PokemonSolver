﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PokemonSolver.Debug;
using PokemonSolver.Interaction;
using PokemonSolver.MapData;
using PokemonSolver.Memory;

namespace PokemonSolver.Algoritm
{
    public class Position : IShortStringable
    {
        private static OverworldEngine _overworldEngine = null;

        public int MapBank { get; private set; }
        public int MapIndex { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public Direction Direction { get; private set; }
        public Altitude Altitude { get; private set; }

        public bool Valid => X >= 0 && X < MapData.Width && Y >= 0 && Y < MapData.Height;
        public Map Map => _overworldEngine.GetMap(this);
        public MapData.MapData MapData => Map.MapData;

        public Position(int bank, int index, int x, int y, Direction direction, Altitude altitude)
        {
            MapBank = bank;
            MapIndex = index;
            X = x;
            Y = y;
            Direction = direction;
            Altitude = altitude;
        }

        public Position(uint mapBank, uint mapIndex, uint x, uint y, Direction direction, Altitude altitude) :
            this((int)mapBank, (int)mapIndex, (int)x, (int)y, direction, altitude)
        {
        }

        public float MinimumDistance(Position goal, Map? nextMapInPath)
        {
            if (nextMapInPath != null)
            {
                var map = _overworldEngine.GetMap(this);
                // Utils.Log($"Searching for connection to map ({nextMapInPath.Bank},{nextMapInPath.MapIndex}) in map ({map.Bank},{map.MapIndex})");
                // foreach (var c in map.Connections)
                // {
                    // Utils.Log($" -> connection : ({c.MapBank},{c.MapIndex},{c.Direction})");
                // }
                var connectionToNextMap = map.Connections.Find(c => c.MapBank == nextMapInPath.Bank && c.MapIndex == nextMapInPath.MapIndex);
                if (connectionToNextMap == null)
                    return float.PositiveInfinity;
                return connectionToNextMap.Direction switch
                {
                    Direction.Down => map.MapData.Height - Y,
                    Direction.Up => Y,
                    Direction.Left => X,
                    Direction.Right => map.MapData.Width - X,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            var distX = X > goal.X ? X - goal.X : goal.X - X;
            var distY = Y > goal.Y ? Y - goal.Y : goal.Y - Y;
            var dirPenalty = Direction == goal.Direction ? 0 : 1;

            return distX + distY + dirPenalty;
        }

        public List<Position> Neighbours()
        {
            var neighbours = new List<Position>(4);
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
            if (_overworldEngine == null)
                throw new Exception("Attempting to compute Forward Position without having called SetMapData in Position.cs");
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
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var p = new Position(MapBank, MapIndex, x, y, Direction, Altitude);
            if (!p.Valid)
                p.FixMap();
            if (!p.Valid)
            {
                // Utils.Log($"Position {p} not valid.");
                return null;
            }

            if (!CanMoveTo(p))
                return null;

            return p; //TODO check altitude change
        }

        public bool CanMoveTo(Position p)
        {
            //check with Route 110 && route 114
            if (_overworldEngine == null)
                throw new Exception("Attempting to compute Forward Position without having called SetMapData in Position.cs");
            var from = MapData.GetTile(X, Y);
            var to = p.MapData.GetTile(p.X, p.Y);
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

        private void FixMap()
        {
            // var map = _overworldEngine.getMap(MapBank, MapIndex).GetNextMap(_overworldEngine, X, Y);
            // if (map == null)
            // return;
            // MapBank = map.Bank;
            // MapIndex = map.MapIndex;
            foreach (var con in _overworldEngine.GetMap(this).Connections)
            {
                var map = _overworldEngine.GetMap(con);
                int baseX, baseY;
                switch (con.Direction)
                {
                    case Direction.Down:
                        baseX = con.Offset;
                        baseY = MapData.Height;
                        break;
                    case Direction.Up:
                        baseX = con.Offset;
                        baseY = -map.MapData.Height;
                        break;
                    case Direction.Left:
                        baseX = -map.MapData.Width;
                        baseY = con.Offset;
                        break;
                    case Direction.Right:
                        baseX = MapData.Width;
                        baseY = con.Offset;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Direction {con.Direction} does not exist");
                }

                if (X < baseX || X >= baseX + map.MapData.Width || Y < baseY || Y >= baseY + map.MapData.Height) continue;

                X -= baseX;
                Y -= baseY;
                MapBank = con.MapBank;
                MapIndex = con.MapIndex;
                return;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Position))
                return false;

            var p = (Position)obj;
            return MapBank == p.MapBank && MapIndex == p.MapIndex && X == p.X && Y == p.Y && Direction == p.Direction;
        }

        public string ToShortString()
        {
            return $"{X},{Y},{Direction.ToString()[0]}";
        }

        public override string ToString()
        {
            return $"Position(({MapBank},{MapIndex}) : {X},{Y},{Direction})";
        }

        public static void SetOverworldEngine(OverworldEngine engine)
        {
            _overworldEngine = engine;
        }
    }
}