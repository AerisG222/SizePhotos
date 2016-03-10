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
            //TODO: update category teaser image if this image is the teaser
            
            _writer.WriteLine("CALL maw_update_photo({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9},"
                                              + " {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19},"
                                              + " {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29},"
                                              + " {30}, {31}, {32}, {33}, {34}, {35}, {36}, {37}, {38}, {39},"
                                              + " {40}, {41}, {42}, {43}, {44}, {45}, {46}, {47}, {48}, {49},"
                                              + " {50}, {51}, {52}, {53}, {54}, {55}, {56}, {57}, {58}, {59},"
                                              + " {60}, {61}, {62}, {63}, {64}, {65});",
                              "@CATEGORY_ID",
                              _category.IsPrivate,
                              result.Xs.Height,
                              result.Xs.Width,
                              result.Sm.Height,
                              result.Sm.Width,
                              result.Lg.Height,
                              result.Lg.Width,
                              result.Md.Height,
                              result.Md.Width,
                              SqlHelper.SqlString(result.Xs.WebPath),
                              SqlHelper.SqlString(result.Sm.WebPath),
                              SqlHelper.SqlString(result.Lg.WebPath),
                              SqlHelper.SqlString(result.Md.WebPath),
                              SqlHelper.SqlString(result.Source.WebPath),
                              SqlHelper.SqlString(result.ExifData.AutofocusPoint),
                              SqlHelper.SqlString(result.ExifData.Aperture),
                              SqlHelper.SqlString(result.ExifData.Contrast),
                              SqlHelper.SqlString(result.ExifData.DepthOfField),
                              SqlHelper.SqlString(result.ExifData.DigitalZoomRatio),
                              SqlHelper.SqlString(result.ExifData.ExposureCompensation),
                              SqlHelper.SqlString(result.ExifData.ExposureDifference),
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
                              SqlHelper.SqlString(result.ExifData.ShotTakenDate),
                              SqlHelper.SqlString(result.ExifData.ExposureProgram),
                              SqlHelper.SqlString(result.ExifData.GpsVersionId),
                              SqlHelper.SqlString(result.ExifData.GpsLatitude),
                              SqlHelper.SqlString(result.ExifData.GpsLatitudeRef),
                              SqlHelper.SqlString(result.ExifData.GpsLongitude),
                              SqlHelper.SqlString(result.ExifData.GpsLongitudeRef),
                              SqlHelper.SqlString(result.ExifData.GpsAltitude),
                              SqlHelper.SqlString(result.ExifData.GpsAltitudeRef),
                              SqlHelper.SqlString(result.ExifData.GpsDateStamp),
                              SqlHelper.SqlString(result.ExifData.GpsTimeStamp),
                              SqlHelper.SqlString(result.ExifData.GpsSatellites));
            
            _writer.WriteLine();
        }
    }
}
