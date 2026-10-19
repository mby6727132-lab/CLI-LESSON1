using System.CommandLine;

var bundleOption = new Option<FileInfo>("--output", "File path and name");

bundleOption.AddAlias("-o");
var languageOption = new Option<string>("--language", "Programming language file ");
languageOption.AddAlias("-l");
var authorOption = new Option<string>("--author", "Name of the file creator");
authorOption.AddAlias("-a");
var languages = new Dictionary<string, string>
{
    { "c#", ".cs" },
    { "java", ".java" },
    { "python", ".py" },
    { "javascript", ".js" },
    { "typescript", ".ts" },
    { "html", ".html" },
    { "css", ".css" },
    { "c++", ".cpp" },
    { "c", ".c" },
    { "sql", ".sql" }
};

var bundleCommand = new Command("bundle", "Bundle code files to a signle files");
bundleCommand.AddOption(bundleOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(authorOption);
bundleCommand.SetHandler((output, language,author) =>
{
    if (output == null)
    {
        Console.Write("enter a name of file to all your code ");
        string filename = Console.ReadLine();


        if (string.IsNullOrWhiteSpace(filename) == true)
        {
            filename = "allFiles.txt";
        }
        output = new FileInfo(filename);
    }

    if (language == null)
    {
        Console.Write("enter a programming language: ");
        language = Console.ReadLine();
    }

    language = language.ToLower();
    if (!languages.ContainsKey(language))
    {
        Console.WriteLine("Language not supported");
        return;
    }
   

    string extension = languages[language];

    try
    {
        File.Create(output.FullName).Close();
    }
    catch (DirectoryNotFoundException)
    {
        Console.WriteLine("Error: the folder in the output path does not exist.");
        return;
    }
    catch (Exception)
    {
        Console.WriteLine("Error: invalid output file name.");
        return;
    }
    if (!string.IsNullOrWhiteSpace(author))
    {
        File.AppendAllText(output.FullName, "// Author: " + author + Environment.NewLine + Environment.NewLine);
    }
    string currentFolder = Directory.GetCurrentDirectory();
    string[] allFiles = Directory.GetFiles(currentFolder, "*" + extension, SearchOption.AllDirectories);
    Array.Sort(allFiles);
    Console.WriteLine(allFiles.Length);
    string[] excludedFolders = { "bin", "obj", "debug", "release", ".vs", ".git", "node_modules" };
    foreach (string file in allFiles)
    {
        string[] pathParts = file.ToLower().Split(Path.DirectorySeparatorChar);
        if (pathParts.Any(part => excludedFolders.Contains(part)))
        {
            continue;
        }
        string readAll = File.ReadAllText(file);
        File.AppendAllText(output.FullName, readAll);
    }

    Console.WriteLine("File was created and full");
}, bundleOption, languageOption, authorOption);

var createRspCommand = new Command("create-rsp", "Create a response file with a ready command");

createRspCommand.SetHandler(() =>
{
    Console.Write("Language (" + string.Join(", ", languages.Keys) + "): ");
    string? language = Console.ReadLine()?.Trim().ToLower();
    while (string.IsNullOrWhiteSpace(language) || !languages.ContainsKey(language))
    {
        Console.Write("Invalid language. Try again: ");
        language = Console.ReadLine().Trim()?.ToLower();
    }

    Console.Write("Output file name: ");
    string? output = Console.ReadLine()?.Trim();
    while (string.IsNullOrWhiteSpace(output))
    {
        Console.Write("File name is required. Try again: ");
        output = Console.ReadLine()?.Trim();
    }

    Console.Write("Author (Enter to skip): ");
    string? author = Console.ReadLine()?.Trim();

    string? command = "bundle -l " + language + " -o \"" + output + "\"";
    if (!string.IsNullOrWhiteSpace(author))
        command += " -a \"" + author + "\"";

    File.WriteAllText("bundle.rsp", command);
    Console.WriteLine("Created bundle.rsp. Run it with: Handle-Code @bundle.rsp");
});
var rootCommad = new RootCommand("Root command for File Bundler CLI");
rootCommad.AddCommand(bundleCommand);
rootCommad.AddCommand(createRspCommand);

await rootCommad.InvokeAsync(args);



