using System.Drawing;

namespace PokemonSolver.Image
{
    namespace Colors
    {
        public class MovementPermissionColors
        {
            private static readonly System.Drawing.Color[] colors = new[]
            {
                Color.Blue, //0
                Color.Red, //1
                Color.GreenYellow, //2
                Color.Cyan, //3
                Color.DeepPink, //4
                Color.Yellow, //5
                Color.Indigo, //6
                Color.Maroon, //7
                Color.DarkKhaki, //8
                Color.DarkGreen, //9
                Color.DarkTurquoise, //A
                Color.DarkBlue, //B
                Color.MediumPurple, //C
                Color.DeepPink, //D
            };

            public static System.Drawing.Color getColorFromPermissionByte(byte permByte)
            {
                if (permByte >= colors.Length)
                    return System.Drawing.Color.White;
                return colors[permByte];
            }
        }

        public class CustomColors
        {
            public static readonly Color Explored = Color.Gray;
            public static readonly Color Path = Color.GreenYellow;
        }
    }
}