namespace SplashGenerator
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    internal static class BitmapGenerator
    {
        public static Stream GenerateSplashBitmap(string targetFileName, string splashResourceName, Stream target)
        {
            var targetAssembly = Assembly.LoadFile(targetFileName);
            var splashControlTypeName = Path.GetFileNameWithoutExtension(splashResourceName);
            var splashControlType = targetAssembly.GetTypes().FirstOrDefault(type => type.Name.Equals(splashControlTypeName, StringComparison.OrdinalIgnoreCase));

            if (splashControlType == null)
                throw new InvalidOperationException($"The project does not contain a type named '{splashControlTypeName}'. Add a user control named {splashControlTypeName}.xaml as a template for your splash screen.");

            var control = splashControlType.GetConstructor(Type.EmptyTypes)?.Invoke(null) as UIElement;

            if (control == null)
                throw new InvalidOperationException($"Type {splashControlType} is not a UIElement with a default constructor. You need to have a user control named {splashControlTypeName}.xaml as a template for your splash screen.");

            return GenerateBitmap(control, target);
        }

        private static Stream GenerateBitmap(UIElement control, Stream target)
        {
            control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var desiredSize = control.DesiredSize;
            control.Arrange(new Rect(0, 0, desiredSize.Width, desiredSize.Height));

            var bitmap = new RenderTargetBitmap((int)desiredSize.Width, (int)desiredSize.Height, 96, 96, PixelFormats.Pbgra32);

            bitmap.Render(control);

            var encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(target);

            return target;
        }
    }
}