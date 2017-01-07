namespace SizePhotos.Minification
{
    public class JpgQualityProcessingResult
        : IProcessingResult
    {
        public bool Successful { get; private set; }
        public uint MinQualitySetting { get; private set; }
        public string ErrorMessage { get; private set; }


        public JpgQualityProcessingResult(bool success, uint minQualitySetting)
        {
            Successful = success;
            MinQualitySetting = minQualitySetting;
        }


        public JpgQualityProcessingResult(string errorMessage)
        {
            Successful = false;
            ErrorMessage = errorMessage;
        }
    }
}
