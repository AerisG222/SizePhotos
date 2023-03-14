using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace SizePhotos;

public class SizePhotoOptions
{
    CategoryInfo _category;

    public bool FastReview { get; private set; }
    public string CategoryName { get; private set; }
    public string Outfile { get; private set; }
    public string LocalPhotoRoot { get; private set; }
    public string WebPhotoRoot { get; private set; }
    public string[] AllowedRoles { get; private set; }
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
                    AllowedRoles = AllowedRoles
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

        if (errors.Any())
        {
            Console.WriteLine("Errors processing options:");

            foreach (var err in errors)
            {
                Console.WriteLine($"  - {err}");
            }

            Console.WriteLine("Exiting");

            Environment.Exit(1);
        }
    }

    // don't love the fact that we update this obj at all, so adding this method
    // so it is easier to see where used until it can be refactored
    public void ResetLocalRoot(string newRoot)
    {
        LocalPhotoRoot = newRoot;
    }

    RootCommand BuildRootCommand()
    {
        var fastReviewOption = new Option<bool>(new[] { "-f", "--fast-review" }, "Quick conversion to review files to keep or throw away");
        var categoryOption = new Option<string>(new[] { "-c", "--category" }, "Name of the category for these photos");
        var outFileOption = new Option<string>(new[] { "-o", "--out-file" }, "Path to the output SQL file that will be generated");
        var photoDirOption = new Option<string>(new[] { "-p", "--photo-dir" }, "Directory containing the source photos");
        var webPhotoRootOption = new Option<string>(new[] { "-w", "--web-photo-root" }, "URL path to the root photos directory, ex: images");
        var allowedRolesOption = new Option<string[]>(new[] { "-r", "--allowed-roles" }, "Roles that will have access to this category");
        var yearOption = new Option<ushort>(new[] { "-y", "--year" }, "Year the pictures were taken");
        var quietOption = new Option<bool>(new[] { "-q", "--quiet" }, "Be quiet and do not emit status messages");
        var insertModeOption = new Option<bool>(new[] { "-i", "--sql-insert-mode" }, "Generate an insert script");
        var updateModeOption = new Option<bool>(new[] { "-u", "--sql-update-mode" }, "Generate an update script (based on lg filepath)");
        var noopModeOption = new Option<bool>(new[] { "-n", "--no-output-mode" }, "Do not generate an output file, useful when reprocessing");

        var rootCommand = new RootCommand("A utility to prepare photos to be shown on mikeandwan.us") {
                fastReviewOption,
                categoryOption,
                outFileOption,
                photoDirOption,
                webPhotoRootOption,
                allowedRolesOption,
                yearOption,
                quietOption,
                insertModeOption,
                updateModeOption,
                noopModeOption
            };

        rootCommand.SetHandler((
            bool fastReview,
            string category,
            string outFile,
            string photoDir,
            string webPhotoRoot,
            string[] allowedRoles,
            ushort year,
            bool quiet,
            bool sqlInsertMode,
            bool sqlUpdateMode,
            bool noOutputMode) =>
        {
            FastReview = fastReview;
            CategoryName = category;
            Outfile = outFile;
            LocalPhotoRoot = photoDir;
            WebPhotoRoot = webPhotoRoot;
            AllowedRoles = allowedRoles;
            Year = year;
            Quiet = quiet;
            InsertMode = sqlInsertMode;
            UpdateMode = sqlUpdateMode;
            NoOutputMode = noOutputMode;
        },
            fastReviewOption,
            categoryOption,
            outFileOption,
            photoDirOption,
            webPhotoRootOption,
            allowedRolesOption,
            yearOption,
            quietOption,
            insertModeOption,
            updateModeOption,
            noopModeOption
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

            if (InsertMode && (AllowedRoles == null || !AllowedRoles.Any()))
            {
                yield return "Please provide at least one role to allow";
            }
        }
    }
}
