using System;
using NMagickWand;


namespace SizePhotos.Optimizer
{
    public interface IPhotoOptimizer
    {
        IOptimizationResult Optimize(MagickWand wand);
    }
}
