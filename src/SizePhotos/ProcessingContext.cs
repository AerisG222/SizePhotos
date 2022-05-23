using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NMagickWand;

namespace SizePhotos;

public class ProcessingContext
{
    readonly List<IProcessingResult> _results = new List<IProcessingResult>();
    public string SourceFile { get; set; }
    public MagickWand Wand { get; set; }

    public IEnumerable<IProcessingResult> Results
    {
        get
        {
            return _results;
        }
    }

    public bool HasErrors
    {
        get
        {
            if (_results.Count == 0)
            {
                return false;
            }

            return _results.Any(x => !x.Successful);
        }
    }

    public IEnumerable<string> ErrorMessages
    {
        get
        {
            return _results
                .Where(x => !x.Successful)
                .Select(x => x.ErrorMessage);
        }
    }

    public ProcessingContext(string photoPath)
    {
        if (string.IsNullOrWhiteSpace(photoPath))
        {
            throw new ArgumentNullException(nameof(photoPath));
        }

        if (!File.Exists(photoPath))
        {
            throw new FileNotFoundException(photoPath);
        }

        SourceFile = photoPath;
    }

    internal void AddResult(IProcessingResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        _results.Add(result);
    }
}
