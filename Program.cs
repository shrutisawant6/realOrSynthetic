using Tesseract;

namespace RealOrSynthetic
{
    internal class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            BeginProcessingFiles();
        }

        private static void BeginProcessingFiles()
        {
            InitialMessage();

            SelectFoldersAndProcessFiles();
        }

        private static void InitialMessage()
        {
            Console.WriteLine("");
            WriteColoredText(" \u25A0 To detect text within an image and move it to a destination folder, press any key. \u25A0");
            Console.ReadKey();
            Console.WriteLine(""); Console.WriteLine("");
        }

        private static void SelectFoldersAndProcessFiles()
        {
            try
            {
                string sourceFolder = SelectFolder("Select Source Folder");
                WriteColoredText($" \u25A0 {sourceFolder}");
                Console.WriteLine("");
                string destinationFolder = SelectFolder("Select Destination Folder");
                WriteColoredText($" \u25A0 {destinationFolder}");

                if (string.IsNullOrEmpty(sourceFolder) && string.IsNullOrEmpty(destinationFolder))
                {
                    WriteColoredText("None of the folders selected.", ConsoleColor.Yellow);
                    Console.ReadKey();
                    return;
                }

                ProcessFiles(sourceFolder, destinationFolder);
            }
            catch
            {
                WriteColoredText("Houston, we have a problem. Try re-launching!", ConsoleColor.Red);
            }

            Console.ReadKey();
        }

        private static void ProcessFiles(string sourceFolder, string destinationFolder)
        {
            int errorCount = 0;
            Console.Clear();
            Console.WriteLine(""); Console.WriteLine("");
            WriteColoredText(" \u25A0 Processing of files has been started... \u25A0", ConsoleColor.Green);
            var files = Directory.GetFiles(sourceFolder).ToList();

            int totalNumberOfFiles = files.Count;
            int successfullyProcessedFiles = 0;

            foreach (var imageFile in files)
            {
                try
                {
                    Console.WriteLine(""); Console.WriteLine("");
                    WriteColoredText($"{imageFile}");
                    WriteColoredText("Processing...", ConsoleColor.Green);

                    string destinationPath = Path.Combine(destinationFolder, Path.GetFileName(imageFile));
                    OCRDetection(imageFile, destinationPath, ref successfullyProcessedFiles);
                }
                catch (Exception ex)
                {
                    WriteColoredText(ex.Message, ConsoleColor.Red);
                    errorCount++;
                }

                Console.Clear();
            }


            Console.WriteLine(""); Console.WriteLine("");
            if(successfullyProcessedFiles == 0)
                WriteColoredText($" \u25A0 None of the files were moved to the destination folder. \u25A0", ConsoleColor.Yellow);
            else if (errorCount > 0)
                WriteColoredText($" \u25A0 Few files({successfullyProcessedFiles}/{totalNumberOfFiles}) are moved to the destination folder successfully. \u25A0", ConsoleColor.Yellow);
            else
                WriteColoredText(" \u25A0 Files are moved to the destination folder successfully. \u25A0", ConsoleColor.Green);

            Console.ReadKey();
        }

        public static string SelectFolder(string title)
        {
            WriteColoredText(title);
            using var dialog = new FolderBrowserDialog();
            dialog.Description = title;
            dialog.UseDescriptionForTitle = true;

            DialogResult result = dialog.ShowDialog();
            return result == DialogResult.OK ? dialog.SelectedPath : string.Empty;
        }

        public static void OCRDetection(string imagePath, string destinationPath, ref int successfullyProcessedFiles)
        {
            string tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
#if DEBUG
            tessDataPath = @$"{Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName}\tessdata";
#endif

            //load the image
            using var img = Pix.LoadFromFile(imagePath);

            //add tesseract languages into engine https://github.com/tesseract-ocr/tessdata
            using var engine = new TesseractEngine(tessDataPath, "hin+eng", EngineMode.Default);
            using var page = engine.Process(img);

            //extract the text
            string text = page.GetText();
            if (!IsOnlyWhitespaceAndNewlines(text))
            {
                File.Move(imagePath, destinationPath);
                successfullyProcessedFiles ++;
            }
        }

        static bool IsOnlyWhitespaceAndNewlines(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            input = input.Trim();
            return input.All(c => c == ' ' || c == '\n');
        }

        private static void WriteColoredText(string message, ConsoleColor color = ConsoleColor.Cyan)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
