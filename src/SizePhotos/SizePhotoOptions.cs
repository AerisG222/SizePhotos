using System;
using System.CommandLine;


namespace SizePhotos
{
    public class SizePhotoOptions
    {
        bool _help;
        bool _fastReview;
        string _categoryName;
        string _outFile;
        string _photoDirectory;
        string _webPhotoRoot;
        bool _isPrivate;
        int _year;
        bool _quiet;
        bool _sqlInsertMode;
        bool _sqlUpdateMode;
        bool _noOutputMode;
        CategoryInfo _category;

        public bool FastReview { get { return _fastReview; } }
        public string CategoryName { get { return _categoryName; } }
        public string Outfile { get { return _outFile; } }
        public string LocalPhotoRoot { get { return _photoDirectory; } }
        public string WebPhotoRoot { get { return _webPhotoRoot; } }
        public bool IsPrivate { get { return _isPrivate; } }
        public ushort Year { get { return (ushort)_year; } }
        public bool Quiet { get { return _quiet; } }
        public bool InsertMode { get { return _sqlInsertMode; } }
        public bool UpdateMode { get { return _sqlUpdateMode; } }
        public bool NoOutputMode { get { return _noOutputMode; } }
        

        public CategoryInfo CategoryInfo
        {
            get
            {
                if (_category == null)
                {
                    _category = new CategoryInfo
                    {
                        Name = CategoryName,
                        Year = Year,
                        IsPrivate = IsPrivate
                    };
                }

                return _category;
            }
        }


        public void Parse(string[] args)
        {
            ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.ApplicationName = "SizePhotos";

                syntax.HandleHelp = false;

                syntax.DefineOption("h|help", ref _help, "help");
                syntax.DefineOption("f|fast-review", ref _fastReview, "Quick conversion to review files to keep or throw away");
                syntax.DefineOption("c|category", ref _categoryName, "Name of the category for these photos");
                syntax.DefineOption("o|out-file", ref _outFile, "Path to the output SQL file that will be generated");
                syntax.DefineOption("p|photo-dir", ref _photoDirectory, "Directory containing the source photos");
                syntax.DefineOption("w|web-photo-root", ref _webPhotoRoot, "URL path to the root photos directory, ex: images");
                syntax.DefineOption("x|private", ref _isPrivate, "Mark the category as private");
                syntax.DefineOption("y|year", ref _year, "Year the pictures were taken");
                syntax.DefineOption("q|quiet", ref _quiet, "Be quiet and do not emit status messages");
                syntax.DefineOption("i|sql-insert-mode", ref _sqlInsertMode, "Generate an insert script");  // SetName = "OutputMode"
                syntax.DefineOption("u|sql-update-mode", ref _sqlUpdateMode, "Generate an update script (based on lg filepath)"); // SetName = "OutputMode"
                syntax.DefineOption("n|no-output-mode", ref _noOutputMode, "Do not generate an output file, useful when reprocessing"); // SetName = "OutputMode"

                if(_help)
                {
                    Console.WriteLine(syntax.GetHelpText());
                    Environment.Exit(0);
                }
                else
                {
                    ValidateOptions(syntax);
                }
            });
        }


        public void ValidateOptions(ArgumentSyntax syntax)
        {
            if (string.IsNullOrWhiteSpace(LocalPhotoRoot))
            {
                syntax.ReportError("Please specify the local path containing the photos to process");
            }

            var counter = 0;

            counter += InsertMode ? 1 : 0;
            counter += UpdateMode ? 1 : 0;
            counter += NoOutputMode ? 1 : 0;

            if (FastReview)
            {
                // we don't need other args, populate required
                _webPhotoRoot = "ignore";
                _noOutputMode = true;
            }
            else if (counter != 1)
            {
                syntax.ReportError("Please select one and only one output mode (insert, update, or no output)");
            }
            else
            {
                if ((InsertMode || UpdateMode) && string.IsNullOrWhiteSpace(WebPhotoRoot))
                {
                    syntax.ReportError("Please specify the web root path");
                }

                if ((InsertMode || UpdateMode) && string.IsNullOrWhiteSpace(Outfile))
                {
                    syntax.ReportError("Please provide the name of the output file to write to");
                }

                if (InsertMode && string.IsNullOrWhiteSpace(CategoryName))
                {
                    syntax.ReportError("Please provide a category name, as it is required for insert mode");
                }

                if (InsertMode && Year == 0)
                {
                    syntax.ReportError("Please provide a year, as it is required for insert mode");
                }
            }
        }


        public PhotoPathHelper GetPathHelper()
        {
            if (FastReview)
            {
                return new PhotoPathHelper(LocalPhotoRoot, WebPhotoRoot, 1);
            }

            if (InsertMode)
            {
                return new PhotoPathHelper(LocalPhotoRoot, WebPhotoRoot, Year);
            }

            return new PhotoPathHelper(LocalPhotoRoot, WebPhotoRoot);
        }
    }
}
