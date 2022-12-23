using System.Drawing;

namespace PokemonSolver.Image
{
    namespace Colors
    {
        public class MovementPermissionColors
        {
            private static readonly System.Drawing.Color[] permissionColors = new[]
            {
                // Color.Gray,
                // Color.Red,
                // Color.Green,
                // Color.Blue,
                // Color.Yellow,
                // Color.Cyan,
                // Color.Magenta,
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
                Color.SaddleBrown, //E
                Color.DarkOrange, //F
                Color.OliveDrab, //10
                Color.LimeGreen, //11
                Color.FromArgb(165, 42, 42), //12
                Color.FromArgb(40, 31, 97), //13
                Color.FromArgb(0, 83, 0), //14
                Color.FromArgb(125, 166, 189), //15
                Color.FromArgb(213, 155, 36), //16
                Color.FromArgb(86, 41, 41), //17
                Color.FromArgb(21,106,98), //18
                Color.FromArgb(171,41,81), //19
                Color.FromArgb(159,232,151), //1A
                Color.FromArgb(46,132,184), //1B
                Color.FromArgb(112,53,191), //1C
                Color.FromArgb(97,117,184), //1D
                Color.FromArgb(50,80,97), //1E
                Color.FromArgb(172,202,53), //1F
                Color.FromArgb(255,255,0), //20
                Color.FromArgb(101,128,64), //21
                Color.FromArgb(204,104,104), //22
                Color.FromArgb(0,128,64), //23
                Color.FromArgb(78,112,248), //24
                Color.FromArgb(49,89,164), //25
                Color.FromArgb(236,154,32), //26
                Color.FromArgb(0,81,81), //27
                Color.FromArgb(180,222,33), //28
                Color.FromArgb(245,75,80), //29
                Color.FromArgb(32,64,32), //2A
                Color.FromArgb(128,255,0), //2B
                Color.FromArgb(30,172,104), //2C
                Color.FromArgb(190,118,65), //2D
                Color.FromArgb(228,214,27), //2E
                Color.FromArgb(47,215,160), //2F
                Color.FromArgb(20,235,165), //30
                Color.FromArgb(128,64,64), //31
                Color.FromArgb(128,64,0), //32
                Color.FromArgb(227,247,117), //33
                Color.FromArgb(200,171,55), //34
                Color.FromArgb(62,193,101), //35
                Color.FromArgb(113,150,44), //36
                Color.FromArgb(128,64,0), //37
                Color.FromArgb(0,255,255), //38
                Color.FromArgb(188,10,192), //39
                Color.FromArgb(53,105,22), //3A
                Color.FromArgb(243,239,92), //3B
                Color.FromArgb(83,85,172), //3C
                Color.FromArgb(69,111,218), //3D
                Color.FromArgb(57,198,146), //3E
                Color.FromArgb(26,102,90), //3F
            };
            
            

            public static Color getColorFromPermissionByte(byte permByte)
            {
                if (permByte >= permissionColors.Length)
                return System.Drawing.Color.White;
                return permissionColors[permByte];
            }
        }

        public class CustomColors
        {
            public static readonly Color Explored = Color.Gray;
            public static readonly Color Path = Color.GreenYellow;
        }
    }
}