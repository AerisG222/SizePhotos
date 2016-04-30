using System;
using System.Threading.Tasks;


namespace SizePhotos
{
    public interface IRawConverter
    {
        bool IsRawFile(string file);
        Task<string> ConvertAsync(string sourceFile);
    }
}
