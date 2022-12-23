using PokemonSolver.Algoritm;

namespace PokemonSolver.Form
{
    public abstract class FormUtils
    {
        public const int DefaultMargin = 5;
        public const int WindowWidth = 600;
        public const int WindowHeight = 480;

        public static Direction GetDirectionFromSelectIndex(int index)
        {
            return (Direction)(index + 1);
        }
        
        public static int GetSelectIndexFromDirection(Direction dir)
        {
            return (int)dir - 1;
        }
    }
}