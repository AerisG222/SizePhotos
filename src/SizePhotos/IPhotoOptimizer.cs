using System;
using NMagickWand;


namespace SizePhotos
{
    public interface IPhotoOptimizer
    {
        void Optimize(MagickWand wand);
    }
}
