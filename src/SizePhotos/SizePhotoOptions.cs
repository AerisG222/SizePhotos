using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;

namespace SizePhotos
{
    public class SizePhotoOptions
    {
        CategoryInfo _category;

        public bool FastReview { get; private set; }
        public string CategoryName { get; private set; }
        public string Outfile { get; private set; }
        public string LocalPhotoRoot { get; private set; }
        public string WebPhotoRoot { get; private set; }
        public bool IsPrivate { get; private set; }
        public ushort Year { get; private set; }
        public bool Quiet { get; private set; }
        public bool InsertMode { get; private set; }
        public bool UpdateMode { get; private set; }
        public bool NoOutputMode { get; private set; }

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
            var rootCommand = BuildRootCommand();

            rootCommand.Invoke(args);

            var errors = ValidateOptions();

            if(errors.Any())
            {
                Console.WriteLine("Errors processing options:");

                foreach(var err in errors)
                {
                    Console.WriteLine($"  - {err}");
                }

                Console.WriteLine("Exiting");

                Environment.Exit(1);
            }
        }

        RootCommand BuildRootCommand()
        {
            var rootCommand = new RootCommand
            {
                new Option<bool>(
                    new string[] {"-f", "--fast-review"},
                    "Quick conversion to review files to keep or throw away"
                ),
                new Option<string>(
                    new string[] {"-c", "--category"},
                    "Name of the category for these photos"
                ),
                new Option<string>(
                    new string[] {"-o", "--out-file"},
                    "Path to the output SQL file that will be generated"
                ),
                new Option<string>(
                    new string[] {"-p", "--photo-dir"},
                    "Directory containing the source photos"
                ),
                new Option<string>(
                    new string[] {"-w", "--web-photo-root"},
                    "URL path to the root photos directory, ex: images"
                ),
                new Option<bool>(
                    new string[] {"-x", "--is-private"},
                    "Mark the category as private"
                ) ,
                new Option<ushort>(
                    new string[] {"-y", "--year"},
                    "Year the pictures were taken"
                ),
                new Option<bool>(
                    new string[] {"-q", "--quiet"},
                    "Be quiet and do not emit status messages"
                ),
                new Option<bool>(
                    new string[] {"-i", "--sql-insert-mode"},
                    "Generate an insert script"  // SetName = "OutputMode"
                ),
                new Option<bool>(
                    new string[] {"-u", "--sql-update-mode"},
                    "Generate an update script (based on lg filepath)" // SetName = "OutputMode"
                ),
                new Option<bool>(
                    new string[] {"-n", "--no-output-mode"},
                    "Do not generate an output file, useful when reprocessing" // SetName = "OutputMode"
                ),
            };

            rootCommand.Description = "A utility to prepare photos to be shown on mikeandwan.us";

            rootCommand.Handler = CommandHandler.Create<bool, string, string, string, string, bool, ushort, bool, bool, bool, bool>(
                (fastReview, category, outFile, photoDir, webPhotoRoot, isPrivate, year, quiet, sqlInsertMode, sqlUpdateMode, noOutputMode) => {
                    FastReview = fastReview;
                    CategoryName = category;
                    Outfile = outFile;
                    LocalPhotoRoot = photoDir;
                    WebPhotoRoot = webPhotoRoot;
                    IsPrivate = isPrivate;
                    Year = year;
                    Quiet = quiet;
                    InsertMode = sqlInsertMode;
                    UpdateMode = sqlUpdateMode;
                    NoOutputMode = noOutputMode;
                }
            );

            return rootCommand;
        }

        IEnumerable<string> ValidateOptions()
        {
            if (string.IsNullOrWhiteSpace(LocalPhotoRoot))
            {
                yield return "Please specify the local path containing the photos to process";
            }

            var counter = 0;

            counter += InsertMode ? 1 : 0;
            counter += UpdateMode ? 1 : 0;
            counter += NoOutputMode ? 1 : 0;

            if (FastReview)
            {
                // we don't need other args, populate required
                WebPhotoRoot = "ignore";
                NoOutputMode = true;
            }
            else if (counter != 1)
            {
                yield return "Please select one and only one output mode (insert, update, or no output)";
            }
            else
            {
                if ((InsertMode || UpdateMode) && string.IsNullOrWhiteSpace(WebPhotoRoot))
                {
                    yield return "Please specify the web root path";
                }

                if ((InsertMode || UpdateMode) && string.IsNullOrWhiteSpace(Outfile))
                {
                    yield return "Please provide the name of the output file to write to";
                }

                if (InsertMode && string.IsNullOrWhiteSpace(CategoryName))
                {
                    yield return "Please provide a category name, as it is required for insert mode";
                }

                if (InsertMode && Year == 0)
                {
                    yield return "Please provide a year, as it is required for insert mode";
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
