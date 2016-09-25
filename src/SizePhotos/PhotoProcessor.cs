using System;
using System.IO;
using System.Threading.Tasks;
using NMagickWand;
using SizePhotos.Exif;
using SizePhotos.Optimizer;
using SizePhotos.Quality;
using SizePhotos.Raw;


namespace SizePhotos
{
    public class PhotoProcessor
        : IPhotoProcessor
    {
        static readonly object _lockobj = new object();
        
        readonly bool _quiet;
        readonly PhotoPathHelper _pathHelper;
        readonly IPhotoOptimizer _optimizer;
        readonly IRawConverter _rawConverter;
        readonly IExifReader _exifReader;
        readonly IQualitySearcher _qualitySearcher;
        
        
        ProcessingTarget SourceTarget { get; set; }
        ProcessingTarget PrintTarget { get; set; }
        ProcessingTarget XsTarget { get; set; }
        ProcessingTarget SmTarget { get; set; }
        ProcessingTarget MdTarget { get; set; }
        ProcessingTarget LgTarget { get; set; }
        
        
        public PhotoProcessor(PhotoPathHelper pathHelper, 
                              IPhotoOptimizer photoOptimizer, 
                              IRawConverter rawConverter, 
                              IExifReader exifReader,
                              IQualitySearcher qualitySearcher,
                              ProcessingTarget sourceTarget,
                              ProcessingTarget printTarget,  
                              ProcessingTarget xsTarget, 
                              ProcessingTarget smTarget, 
                              ProcessingTarget mdTarget, 
                              ProcessingTarget lgTarget, 
                              bool quiet)
        {
            _quiet = quiet;
            _pathHelper = pathHelper;
            _optimizer = photoOptimizer;
            _rawConverter = rawConverter;
            _exifReader = exifReader;
            _qualitySearcher = qualitySearcher;
            
            SourceTarget = sourceTarget;
            PrintTarget = printTarget;
            XsTarget = xsTarget;
            SmTarget = smTarget;
            MdTarget = mdTarget;
            LgTarget = lgTarget;
        }
        
        
        public async Task<ProcessingResult> ProcessPhotoAsync(string filename)
        {
            var result = new ProcessingResult();
            var jpgName = Path.ChangeExtension(filename, ".jpg");
            var origPath = _pathHelper.GetSourceFilePath(filename);
            var srcPath = _pathHelper.GetScaledLocalPath(SourceTarget.ScaledPathSegment, filename);
            
            result.ExifData = await _exifReader.ReadExifDataAsync(origPath);
            
            // always keep the original in the source dir
            File.Move(origPath, srcPath);
            result.Source = new ProcessedPhoto { 
                Target = SourceTarget, 
                LocalFilePath = srcPath, 
                WebFilePath = _pathHelper.GetScaledWebFilePath(SourceTarget.ScaledPathSegment, filename)
            };
            
            using(var wand = new MagickWand())
            {
                if(_rawConverter.IsRawFile(srcPath))
                {
                    result.RawConversionResult = await _rawConverter.ConvertAsync(srcPath);
                    
                    wand.ReadImage(result.RawConversionResult.OutputFile);
                    File.Delete(result.RawConversionResult.OutputFile);
                } 
                else 
                {
                    wand.ReadImage(srcPath);
                }
                
                result.Source.Height = wand.ImageHeight;
                result.Source.Width = wand.ImageWidth;
                
                wand.AutoOrientImage();
                wand.StripImage();
                
                using(var optWand = wand.Clone())
                {
                    result.OptimizationResult = _optimizer.Optimize(optWand);
                    
                    // get the best compression quality for the optimized image
                    // (best => smallest size for negligible quality loss)
                    result.CompressionQuality = (short)_qualitySearcher.GetOptimalQuality(optWand);
                    
                    result.Xs = ProcessTarget(wand, optWand, result.CompressionQuality, XsTarget, jpgName);
                    result.Sm = ProcessTarget(wand, optWand, result.CompressionQuality, SmTarget, jpgName);
                    result.Md = ProcessTarget(wand, optWand, result.CompressionQuality, MdTarget, jpgName);
                    result.Lg = ProcessTarget(wand, optWand, result.CompressionQuality, LgTarget, jpgName);
                    result.Print = ProcessTarget(wand, optWand, result.CompressionQuality, PrintTarget, jpgName);
                }
            }
            
            return result;
        }
        
        
        ProcessedPhoto ProcessTarget(MagickWand wand, 
                                     MagickWand optimizedWand, 
                                     short adjustedQuality, 
                                     ProcessingTarget target, 
                                     string jpgName)
        {
            var srcWand = target.Optimize ? optimizedWand : wand;
            
            using(var tmpWand = srcWand.Clone())
            {
                var path = _pathHelper.GetScaledLocalPath(target.ScaledPathSegment, jpgName);
                uint width, height;
                
                if(target.MaxWidth > 0)
                {
                    tmpWand.GetLargestDimensionsKeepingAspectRatio(target.MaxWidth, target.MaxHeight, out width, out height);
                    
                    tmpWand.ScaleImage(width, height);
                }
                else
                {
                    width = wand.ImageWidth;
                    height = wand.ImageHeight;
                }
                
                // sharpen after potentially resizing
                // http://www.imagemagick.org/Usage/resize/#resize_unsharp
                tmpWand.UnsharpMaskImage(0, 0.7, 0.7, 0.008);
                
                if(target.AdjustQuality)
                {
                    tmpWand.ImageCompressionQuality = Convert.ToUInt32(adjustedQuality);
                }
                
                tmpWand.WriteImage(path, true);
                
                return new ProcessedPhoto
                {
                    Target = target,
                    LocalFilePath = jpgName, 
                    WebFilePath = _pathHelper.GetScaledWebFilePath(target.ScaledPathSegment, jpgName),
                    Width = width,
                    Height = height
                };
            }
        }
    }
}
