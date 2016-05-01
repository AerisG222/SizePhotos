using System;
using System.Threading.Tasks;


namespace SizePhotos.Raw
{
    public interface IRawConverter
    {
        bool IsRawFile(string file);
        Task<IRawConversionResult> ConvertAsync(string sourceFile);
    }
}
