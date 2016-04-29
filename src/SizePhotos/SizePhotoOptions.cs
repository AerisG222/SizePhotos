using System.Collections.Generic;
using CommandLine;


namespace SizePhotos
{
    public class SizePhotoOptions
    {
        [Option('c', "category", HelpText = "Name of the category for these photos")]
        public string CategoryName { get; set;}
        
        
        [Option('o', "out-file", HelpText = "Path to the output SQL file that will be generated")]
        public string Outfile { get; set; }
        
        
        [Option('p', "photo-dir", HelpText = "Directory containing the source photos")]
        public string LocalPhotoRoot { get; set; }
        
        
        [Option('w', "web-photo-root", HelpText = "URL path to the root photos directory, ex: images")]
        public string WebPhotoRoot { get; set; }

        
        [Option('x', "private", HelpText = "Mark the category as private")]
        public bool IsPrivate { get; set; }
        
        
        [Option('y', "year", HelpText = "Year the pictures were taken")]
        public ushort Year { get; set; }
        
        
        [Option('q', "quiet", HelpText = "Be quiet and do not emit status messages")]
        public bool Quiet { get; set; }
        
        
        [Option('i', "sql-insert-mode", SetName = "OutputMode", HelpText = "Generate an insert script")]
        public bool InsertMode { get; set; }
        
        
        [Option('u', "sql-update-mode", SetName = "OutputMode", HelpText = "Generate an update script (based on lg filepath)")]
        public bool UpdateMode { get; set; }
        
        
        [Option('n', "no-output-mode", SetName = "OutputMode", HelpText = "Do not generate an output file, useful when reprocessing")]
        public bool NoOutputMode { get; set; }
        
        
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
                if((InsertMode || UpdateMode) && string.IsNullOrWhiteSpace(WebPhotoRoot))
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
        
        
        public PhotoPathHelper GetPathHelper()
        {
            if(InsertMode)
            {
                return new PhotoPathHelper(LocalPhotoRoot, WebPhotoRoot, Year);
            }
            
            return new PhotoPathHelper(LocalPhotoRoot, WebPhotoRoot);
        }
    }
}
