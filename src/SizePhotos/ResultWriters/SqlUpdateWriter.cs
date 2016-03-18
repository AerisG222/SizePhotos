using System.IO;


namespace SizePhotos.ResultWriters
{
    public class SqlUpdateWriter
        : IResultWriter
    {
        readonly string _file;
        CategoryInfo _category;
        StreamWriter _writer;

        
        public SqlUpdateWriter(string outputFile)
        {
            _file = outputFile;
        }
        
        
        public void PreProcess(CategoryInfo category)
        {
            _category = category;
            PrepareOutputStream();
        }
        
        
        public void PostProcess()
        {
            FinalizeOutputStream();
        }
        
        
        public void Write(ProcessingResult result)
        {
            AddResultToOutput(result);
        }
        
        
        void PrepareOutputStream()
        {
            _writer = new StreamWriter(new FileStream(_file, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 8192, FileOptions.None));
        }
        
        
        void FinalizeOutputStream()
        {
            _writer.Flush();
            _writer.Close();
            _writer = null;
        }
        
        
        void AddResultToOutput(ProcessingResult result)
        {
            var args = new string[] {
                _category.IsPrivate.ToString(),
                result.Xs.Height.ToString(),
                result.Xs.Width.ToString(),
                result.Sm.Height.ToString(),
                result.Sm.Width.ToString(),
                result.Lg.Height.ToString(),
                result.Lg.Width.ToString(),
                result.Md.Height.ToString(),
                result.Md.Width.ToString(),
                SqlHelper.SqlString(result.Xs.WebPath),
                SqlHelper.SqlString(result.Sm.WebPath),
                SqlHelper.SqlString(result.Lg.WebPath),
                SqlHelper.SqlString(result.Md.WebPath),
                SqlHelper.SqlString(result.Source.WebPath),
                SqlHelper.SqlString(result.ExifData.AutoFocusPoint),
                SqlHelper.SqlString(result.ExifData.Aperture),
                SqlHelper.SqlString(result.ExifData.Contrast),
                SqlHelper.SqlString(result.ExifData.DepthOfField),
                SqlHelper.SqlString(result.ExifData.DigitalZoomRatio),
                SqlHelper.SqlString(result.ExifData.ExposureCompensation),
                SqlHelper.SqlString(result.ExifData.ExposureMode),
                SqlHelper.SqlString(result.ExifData.ExposureTime),
                SqlHelper.SqlString(result.ExifData.FNumber),
                SqlHelper.SqlString(result.ExifData.Flash),
                SqlHelper.SqlString(result.ExifData.FlashExposureCompensation),
                SqlHelper.SqlString(result.ExifData.FlashMode),
                SqlHelper.SqlString(result.ExifData.FlashSetting),
                SqlHelper.SqlString(result.ExifData.FlashType),
                SqlHelper.SqlString(result.ExifData.FocalLength),
                SqlHelper.SqlString(result.ExifData.FocalLengthIn35mmFormat),
                SqlHelper.SqlString(result.ExifData.FocusDistance),
                SqlHelper.SqlString(result.ExifData.FocusMode),
                SqlHelper.SqlString(result.ExifData.FocusPosition),
                SqlHelper.SqlString(result.ExifData.GainControl),
                SqlHelper.SqlString(result.ExifData.HueAdjustment),
                SqlHelper.SqlString(result.ExifData.HyperfocalDistance),
                SqlHelper.SqlString(result.ExifData.Iso),
                SqlHelper.SqlString(result.ExifData.LensId),
                SqlHelper.SqlString(result.ExifData.LightSource),
                SqlHelper.SqlString(result.ExifData.Make),
                SqlHelper.SqlString(result.ExifData.MeteringMode),
                SqlHelper.SqlString(result.ExifData.Model),
                SqlHelper.SqlString(result.ExifData.NoiseReduction),
                SqlHelper.SqlString(result.ExifData.Orientation),
                SqlHelper.SqlString(result.ExifData.Saturation),
                SqlHelper.SqlString(result.ExifData.ScaleFactor35Efl),
                SqlHelper.SqlString(result.ExifData.SceneCaptureType),
                SqlHelper.SqlString(result.ExifData.SceneType),
                SqlHelper.SqlString(result.ExifData.SensingMethod),
                SqlHelper.SqlString(result.ExifData.Sharpness),
                SqlHelper.SqlString(result.ExifData.ShutterSpeed),
                SqlHelper.SqlString(result.ExifData.WhiteBalance),
                SqlHelper.SqlString(result.ExifData.ExposureProgram),
                SqlHelper.SqlString(result.ExifData.GpsVersionId),
                SqlHelper.SqlString(result.ExifData.GpsLatitude),
                SqlHelper.SqlString(result.ExifData.GpsLatitudeRef),
                SqlHelper.SqlString(result.ExifData.GpsLongitude),
                SqlHelper.SqlString(result.ExifData.GpsLongitudeRef),
                SqlHelper.SqlString(result.ExifData.GpsAltitude),
                SqlHelper.SqlString(result.ExifData.GpsAltitudeRef),
                SqlHelper.SqlString(result.ExifData.GpsDateStamp),
                SqlHelper.SqlString(result.ExifData.GpsSatellites)
            };
            
            _writer.WriteLine($"CALL maw_update_photo({string.Join(", ", args)});");
            _writer.WriteLine();
        }
    }
}
