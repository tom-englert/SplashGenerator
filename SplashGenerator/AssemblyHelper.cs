namespace SplashGenerator
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Resources;

    using Mono.Cecil;

    internal static class AssemblyHelper
    {
        public static void ReplaceResource(string assemblyFileName, string resourceName, Stream resourceData)
        {
            var definition = AssemblyDefinition.ReadAssembly(assemblyFileName, new ReaderParameters(ReadingMode.Immediate) { ReadSymbols = true });
            var moduleResources = definition.MainModule.Resources;

            var originalResource = moduleResources
                .OfType<EmbeddedResource>()
                .FirstOrDefault(r => r.Name.EndsWith(@".g.resources", StringComparison.OrdinalIgnoreCase));
            if (originalResource == null)
                throw new InvalidOperationException($"The assembly '{assemblyFileName}' does not seem to be a valid WPF executable, it does not contain WPF resources.");

            var newResourceData = ReplaceResource(originalResource.GetResourceStream(), resourceName, resourceData);
            var newResource = new EmbeddedResource(originalResource.Name, originalResource.Attributes, newResourceData);

            moduleResources.Remove(originalResource);
            moduleResources.Add(newResource);

            definition.Write(assemblyFileName, new WriterParameters { WriteSymbols = true });
        }

        private static byte[] ReplaceResource(Stream originalResource, string resourceName, Stream resourceData)
        {
            using (var targetStream = new MemoryStream())
            {
                using (var writer = new ResourceWriter(targetStream))
                {
                    using (var reader = new ResourceReader(originalResource))
                    {
                        foreach (DictionaryEntry item in reader)
                        {
                            var key = (string)item.Key;

                            if (resourceName.Equals(key, StringComparison.OrdinalIgnoreCase))
                            {
                                writer.AddResource(key, resourceData);
                            }
                            else
                            {
                                writer.AddResource(key, (Stream)item.Value);
                            }
                        }
                    }
                }

                return targetStream.GetBuffer();
            }
        }
    }
}