using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SizePhotos.ResultWriters
{
    public class SqlInsertWriter
        : IResultWriter
    {
        readonly string _file;
        CategoryInfo _category;
        StreamWriter _writer;
        bool _hasSetTeaserPhoto;

        
        public SqlInsertWriter(string outputFile)
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
            if(!_hasSetTeaserPhoto)
            {
                _writer.WriteLine(string.Concat("INSERT INTO photo_category (name, year, is_private, teaser_photo_width, teaser_photo_height, teaser_photo_path) ",
                                                "VALUES (", SqlHelper.SqlString(_category.Name), ", ", _category.Year, ", ", _category.IsPrivate, ", ", result.Xs.Width, ", ", result.Xs.Height, ", ", SqlHelper.SqlString(result.Xs.WebPath), ");"));

                _writer.WriteLine();
                _writer.WriteLine(@"SELECT @CATEGORY_ID := LAST_INSERT_ID();");
                _writer.WriteLine();

                _hasSetTeaserPhoto = true;
            }

            var args = new string[] {
                "@CATEGORY_ID",
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
            
            _writer.WriteLine($"CALL maw_add_photo({string.Join(", ", args)});");
            _writer.WriteLine();
        }
    }
}
