namespace SplashGenerator
{
    using System;
    using System.IO;
    using System.Linq;

    internal class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                var splashResourceName = args.FirstOrDefault();
                var targetPath = args.Skip(1).FirstOrDefault();

                if (string.IsNullOrEmpty(splashResourceName) || string.IsNullOrEmpty(targetPath))
                    throw new InvalidOperationException("Expected arguments: <splash image name> <target file name>");

                var targetDirectory = Path.GetDirectoryName(targetPath);

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
