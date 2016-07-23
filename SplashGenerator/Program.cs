namespace SplashGenerator
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.Build.Evaluation;

    internal class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                var projectFileName = args.FirstOrDefault();
                var targetPath = args.Skip(1).FirstOrDefault();

                if (string.IsNullOrEmpty(projectFileName) || string.IsNullOrEmpty(targetPath))
                    throw new InvalidOperationException("Expected arguments: <project file> <target file name>");

                var targetDirectory = Path.GetDirectoryName(targetPath);

                var projectCollection = ProjectCollection.GlobalProjectCollection;
                var project = projectCollection?.LoadProject(projectFileName);
                if (project == null)
                    throw new InvalidOperationException($"'{projectFileName}' does not seem to be a valid project file.");

                var splashResourceName = project.GetItems(@"SplashScreen")
                    .Select(item => item.EvaluatedInclude)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(splashResourceName))
                    throw new InvalidOperationException($"The project '{projectFileName}' does not contain a splash screen image. Add an image to the project and set it's build action to SplashScreen.");

                using (var bitmapData = new MemoryStream())
                {
                    new AppDomainHelper(targetDirectory)
                        .InvokeInSeparateDomain(BitmapGenerator.GenerateBitmap, targetPath, splashResourceName, bitmapData);

                    AssemblyHelper.ReplaceResource(targetPath, splashResourceName, bitmapData);
                }

                return 0;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                return 3;
            }
        }
    }
}
