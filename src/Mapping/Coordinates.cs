﻿namespace PokemonSolver.Mapping
{
    public class Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }


        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public static Coordinates operator +(Coordinates c1, Coordinates c2)
        {
            return new Coordinates(c1.X + c2.X, c1.Y + c2.Y);
        }

        public static Coordinates operator -(Coordinates c1, Coordinates c2)
        {
            return new Coordinates(c1.X - c2.X, c1.Y - c2.Y);
        }

        public static Coordinates Zero => new(0, 0);
    }
}