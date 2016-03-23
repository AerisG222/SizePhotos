using System;
using System.IO;


namespace SizePhotos
{
    public class PhotoPathHelper
    {
        string LocalRoot { get; set; }
        
        string WebRoot { get; set; }
        
        ushort Year { get; set; }
        
        string CategorySegment 
        { 
            get
            {
                var parts = LocalRoot.Split(Path.DirectorySeparatorChar);
                
                if(parts.Length > 0)
                {
                    return parts[parts.Length - 1];
                }
                
                throw new InvalidDataException("invalid local root path: " + LocalRoot);
            }
        }        
        
        
        public PhotoPathHelper(string localRoot, string webRoot, ushort year)
        {
            if(string.IsNullOrWhiteSpace(localRoot))
            {
                throw new ArgumentNullException(nameof(localRoot));
            }
            if(string.IsNullOrWhiteSpace(webRoot))
            {
                throw new ArgumentNullException(nameof(webRoot));
            }
            if(year == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(year));
            }
            
            LocalRoot = localRoot;
            WebRoot = $"/{TrimWebPathSeparators(webRoot)}";
            Year = year;
        }
        
        
        public PhotoPathHelper(string localRoot, string webRoot)
        {
            // this is for the upgrade case, where we should be able to infer the year from the path
            if(string.IsNullOrWhiteSpace(localRoot))
            {
                throw new ArgumentNullException(nameof(localRoot));
            }
            if(string.IsNullOrWhiteSpace(webRoot))
            {
                throw new ArgumentNullException(nameof(webRoot));
            }
            
            LocalRoot = localRoot;
            WebRoot = $"/{TrimWebPathSeparators(webRoot)}";
            
            InferYear();            
        }
        
        
        public string GetSourceFilePath(string filename)
        {
            return Path.Combine(LocalRoot, filename);
        }
        
        
        public string GetScaledLocalPath(string scaleName)
        {
            return Path.Combine(LocalRoot, scaleName);
        }
        
        
        public string GetScaledLocalPath(string scaleName, string filename)
        {
            return Path.Combine(LocalRoot, scaleName, filename);
        }
        
        
        public string GetScaledWebFilePath(string scaleName, string filename)
        {
            return $"{WebRoot}/{Year}/{CategorySegment}/{scaleName}/{filename}";
        }
        
        
        void InferYear()
        {
            var segments = LocalRoot.Split(Path.DirectorySeparatorChar);
            
            if(segments.Length >= 2) 
            {
                ushort s;
                
                if(ushort.TryParse(segments[segments.Length - 2], out s))
                {
                    Year = s;
                }
                else
                {
                    throw new InvalidDataException("unable to infer year: year path segment is not a valid year (in yyyy) format: " + LocalRoot);
                }
            }
            else
            {
                throw new InvalidDataException("unable to infer year: local path does not include a year path segment before the category: " + LocalRoot);
            }
        }
        
        
        string TrimWebPathSeparators(string val)
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
