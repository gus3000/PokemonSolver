namespace PokemonSolver.Gpu
{
    public class GpuMapImage: GpuImage
    {
        public GpuMapImage() : base(GpuFunction.MapImage)
        {
            ImageWidth = 1920;
            ImageHeight = 1080;
        }
        
    }
}