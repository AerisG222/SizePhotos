using System;
using System.IO;
using System.Threading.Tasks;


namespace SizePhotos;

public class MovePhotoProcessor
    : IOutput, IPhotoProcessor
{
    bool _quiet;
    string _subdir;
    bool _updateSource;


    public string OutputSubdirectory
    {
        get
        {
            return _subdir;
        }
    }


    public MovePhotoProcessor(bool quiet, string subdir, bool updateSource)
    {
        _quiet = quiet;
        _subdir = subdir;
        _updateSource = updateSource;
    }


    public IPhotoProcessor Clone()
    {
        return (IPhotoProcessor)MemberwiseClone();
    }


    public Task<IProcessingResult> ProcessPhotoAsync(ProcessingContext context)
    {
        try
        {
            var newPath = Path.Combine(Path.GetDirectoryName(context.SourceFile), _subdir, Path.GetFileName(context.SourceFile));
            var pp3Path = $"{context.SourceFile}.pp3";

            File.Move(context.SourceFile, newPath);

            if (File.Exists(pp3Path))
            {
                var newPp3Path = Path.Combine(Path.GetDirectoryName(pp3Path), _subdir, Path.GetFileName(pp3Path));

                File.Move(pp3Path, newPp3Path);
            }

            if (_updateSource)
            {
                context.SourceFile = newPath;
            }

            return Task.FromResult((IProcessingResult)new MoveProcessingResult(true));
        }
        catch (Exception ex)
        {
            return Task.FromResult((IProcessingResult)new MoveProcessingResult($"Error moving photo to subdir {_subdir}.  Error: {ex.Message}"));
        }
    }
}
