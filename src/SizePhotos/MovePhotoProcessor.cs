using System;
using System.IO;
using System.Threading.Tasks;


namespace SizePhotos
{
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
            return (IPhotoProcessor) MemberwiseClone();
        }


        public Task<IProcessingResult> ProcessPhotoAsync(ProcessingContext context)
        {
            try
            {
                var newPath = Path.Combine(Path.GetDirectoryName(context.SourceFile), _subdir, Path.GetFileName(context.SourceFile));

                File.Move(context.SourceFile, newPath);

                if(_updateSource)
                {
                    context.SourceFile = newPath;
                }
                
                return Task.FromResult((IProcessingResult) new MoveProcessingResult(true));
            }
            catch(Exception ex)
            {
                if(!_quiet)
                {
                    Console.WriteLine($"Error moving photo {context.SourceFile} to subdir {_subdir}.  Error: {ex.Message}");
                }

                return Task.FromResult((IProcessingResult) new MoveProcessingResult(false));
            }
        }
    }
}
