using System.Drawing;

namespace PokemonSolver.Image
{
    public class MovementPermissionColors
    {
        private static readonly Color[] colors = new[]
        {
            Color.Blue,
            Color.Red,
            Color.GreenYellow,
            Color.Cyan,
            Color.DeepPink,
            Color.Yellow,
            Color.Indigo,
            Color.Maroon,
            Color.DarkKhaki,
            Color.DarkGreen,
            Color.DarkTurquoise,
            Color.DarkBlue,
            Color.MediumPurple,
        };

        public static Color getColorFromPermissionByte(byte permByte)
        {
            if (permByte >= colors.Length)
                return Color.White;
            return colors[permByte];
        }
    }
}