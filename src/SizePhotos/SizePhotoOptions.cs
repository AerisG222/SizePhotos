using System;
using System.Collections.Generic;
using System.IO;
using Commons.GetOptions;


namespace SizePhotos
{
    public class SizePhotoOptions 
        : Options
    {
        string _webPhotoRootPath = string.Empty;
        string _categorySegment = string.Empty;
        string _localPhotoRootPath = string.Empty;
        
        
        public SizePhotoOptions(string[] args)
            : base(new OptionsContext())
        {
            
        }
        
        
        [Option("Name of the category for these photos", ShortForm = 'c', Name = "category" )]
        public string CategoryName { get; set;}
        
        
        [Option("Path to the output SQL file that will be generated",  ShortForm = 'o', Name = "out-file")]
        public string Outfile { get; set; }
        
        
        [Option("Directory containing the source photos", ShortForm = 'p', Name = "photo-dir")]
        public string LocalPhotoRoot
        { 
            get
            {
                return _localPhotoRootPath;
            } 
            set
            {
                _localPhotoRootPath = value;
                
                if(!string.IsNullOrEmpty(value))
                {
                    string[] dirComponents = value.Split(Path.DirectorySeparatorChar);

                    CategoryDirectorySegment = dirComponents[dirComponents.Length - 1];
                }
            } 
        }
        
        
        [Option("URL path to the root photos directory, ex: images", ShortForm = 'w', Name = "web-photo-root")]
        public string WebPhotoRootPath 
        { 
            get
            {
                return _webPhotoRootPath;
            } 
            set
            {
                if(string.IsNullOrWhiteSpace(value))
                {
                    _webPhotoRootPath = string.Empty;
                    return;
                }
                
                _webPhotoRootPath = TrimPathSeparators(value);
            } 
        }
        
        
        [Option("Mark the category as private", ShortForm = 'x', Name = "private")]
        public bool IsPrivate { get; set; }
        
        
        [Option("Year the pictures were taken", ShortForm = 'y', Name = "year")]
        public ushort Year { get; set; }
        
        
        [Option("Be quiet and do not emit status messages", ShortForm = 'q', Name = "quiet")]
        public bool Quiet { get; set; }
        
        
        [Option("Generate an insert script [default]", ShortForm = 'i', Name = "sql-insert-mode")]
        public bool InsertMode { get; set; }
        
        
        [Option("Generate an update script (based on lg filepath)", ShortForm = 'u', Name = "sql-update-mode")]
        public bool UpdateMode { get; set; }
        
        
        [Option("Do not generate an output file, useful when reprocessing", ShortForm = 'n', Name = "no-output-mode")]
        public bool NoOutputMode { get; set; }
        
        
        string CategoryDirectorySegment 
        { 
            get
            {
                return _categorySegment;
            } 
            set
            {
                if(string.IsNullOrWhiteSpace(value))
                {
                    _categorySegment = string.Empty;
                    return;
                }
                
                _categorySegment = TrimPathSeparators(value);
            } 
        }
        
        
        public new void ProcessArgs(string[] args, Func<int, string[]> exitFunc)
        {
            base.ProcessArgs(args, null);
            
            // when updating, the expectation is that the directory will match the expected path name convention
            // as such, determine the year based on the localpath (this is necessary to generate proper webpaths in sql)
            if(UpdateMode && Year == 0)
            {
                var parts = LocalPhotoRoot.Split(Path.DirectorySeparatorChar);
                Year = ushort.Parse(parts[parts.Length - 2]);
            }
        }
        
        
        public IEnumerable<string> ValidateOptions()
        {
            if(string.IsNullOrWhiteSpace(LocalPhotoRoot))
            {
                yield return "Please specify the local path containing the photos to process";
            }
            
            var counter = 0;
            
            counter += InsertMode ? 1 : 0;
            counter += UpdateMode ? 1 : 0;
            counter += NoOutputMode ? 1 : 0;
            
            if(counter != 1)
            {
                yield return "Please select one and only one output mode (insert, update, or no output)";
            }
            else
            {
                if((InsertMode || UpdateMode) && string.IsNullOrWhiteSpace(WebPhotoRootPath))
                {
                    yield return "Please specify the web root path";
                }
                
                if((InsertMode || UpdateMode) && string.IsNullOrWhiteSpace(Outfile))
                {
                    yield return "Please provide the name of the output file to write to";
                }
                
                if(InsertMode && string.IsNullOrWhiteSpace(CategoryName))
                {
                    yield return "Please provide a category name, as it is required for insert mode";
                }
                
                if(InsertMode && Year == 0)
                {
                    yield return "Please provide a year, as it is required for insert mode";
                }
            }
        }

        
        public string WebCategoryPath
        {
            get
            {
                return $"/{WebPhotoRootPath}/{Year}/{CategoryDirectorySegment}/";
            }
        }
        
        
        public string GetLocalScaledPath(string scaledPathSegment)
        {
            return $"{Path.Combine(LocalPhotoRoot, scaledPathSegment)}{Path.DirectorySeparatorChar}";
        }
        
        
        public string GetWebScaledPath(string scaledPathSegment)
        {
            return $"{WebCategoryPath}{scaledPathSegment}/";
        }
        
        
        string TrimPathSeparators(string val)
        {
            val = val.Trim();
                
            while(val.StartsWith("/"))
            {
                val = val.Substring(1);
            }
            
            while(val.EndsWith("/"))
            {
                val = val.Substring(0, val.LastIndexOf('/'));
            }
            
            return val;
        }
    }
}
