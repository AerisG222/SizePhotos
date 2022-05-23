using System;


namespace SizePhotos.PhotoReaders;

static class RawHelper
{
    public static bool IsRawFile(string file)
    {
        return file.EndsWith("nef", StringComparison.OrdinalIgnoreCase);
    }
}
