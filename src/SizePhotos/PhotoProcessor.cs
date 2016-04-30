using System;
using System.IO;
using System.Threading.Tasks;
using NMagickWand;


namespace SizePhotos
{
    public class PhotoProcessor
    {
        static readonly object _lockobj = new object();
        
        readonly bool _quiet;
        readonly PhotoPathHelper _pathHelper;
        readonly IPhotoOptimizer _optimizer;
        readonly IRawConverter _rawConverter;
        readonly IExifReader _exifReader;
        
        
        ProcessingTarget SourceTarget { get; set; }
        ProcessingTarget XsTarget { get; set; }
        ProcessingTarget SmTarget { get; set; }
        ProcessingTarget MdTarget { get; set; }
        ProcessingTarget LgTarget { get; set; }
        
        
        
        public PhotoProcessor(PhotoPathHelper pathHelper, IPhotoOptimizer photoOptimizer, IRawConverter rawConverter, IExifReader exifReader,
                              ProcessingTarget sourceTarget, ProcessingTarget xsTarget, ProcessingTarget smTarget, ProcessingTarget mdTarget, 
                              ProcessingTarget lgTarget, bool quiet)
        {
            _quiet = quiet;
            _pathHelper = pathHelper;
            _optimizer = photoOptimizer;
            _rawConverter = rawConverter;
            _exifReader = exifReader;
            
            SourceTarget = sourceTarget;
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
            string ppmFile = null;
            
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
                    ppmFile = await _rawConverter.ConvertAsync(srcPath);
                    
                    wand.ReadImage(ppmFile);
                    File.Delete(ppmFile);
                } 
                else 
                {
                    wand.ReadImage(srcPath);
                }
                
                wand.AutoOrientImage();
                wand.StripImage();
                
                using(var optWand = wand.Clone())
                {
                    _optimizer.Optimize(optWand);
                    
                    result.Xs = ProcessTarget(wand, optWand, XsTarget, jpgName);
                    result.Sm = ProcessTarget(wand, optWand, SmTarget, jpgName);
                    result.Md = ProcessTarget(wand, optWand, MdTarget, jpgName);
                    result.Lg = ProcessTarget(wand, optWand, LgTarget, jpgName);
                }
            }
            
            return result;
        }
        
        
        ProcessedPhoto ProcessTarget(MagickWand wand, MagickWand optimizedWand, ProcessingTarget target, string jpgName)
        {
            var srcWand = target.Optimize ? optimizedWand : wand;
            
            using(var tmpWand = srcWand.Clone())
            {
                var path = _pathHelper.GetScaledLocalPath(target.ScaledPathSegment, jpgName);
                uint width, height;
                
                tmpWand.GetLargestDimensionsKeepingAspectRatio(target.MaxWidth, target.MaxHeight, out width, out height);
                
                tmpWand.ScaleImage(width, height);
                tmpWand.UnsharpMaskImage(0, 0.7, 0.7, 0.008);

                if(target.Quality != null)
                {
                    tmpWand.ImageCompressionQuality = (uint)target.Quality;
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
