using System;
using NMagickWand;


namespace SizePhotos.Quality
{
    public interface IQualitySearcher
    {
        uint GetOptimalQuality(MagickWand wand);
    }
}
