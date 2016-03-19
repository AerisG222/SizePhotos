using System;
using System.IO;
using Commons.GetOptions;


namespace SizePhotos
{
    public class SizePhotoOptions 
        : Options
    {
        string _webPhotoRootPath;
        string _categorySegment;
        string _localPhotoRootPath;
        
        
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
        
        
        public bool ValidateOptions()
        {
            if(WebPhotoRootPath == null ||
               string.IsNullOrEmpty(Outfile) ||
               string.IsNullOrEmpty(LocalPhotoRoot) ||
               string.IsNullOrEmpty(CategoryName) ||
               Year == 0)
            {
                return false;
            }
            
            return true;
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
