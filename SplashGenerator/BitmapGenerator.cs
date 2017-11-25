using System.Runtime.InteropServices;
using System.Windows.Threading;

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
        public static Stream GenerateBitmap(string targetFileName, string resourceName, Stream target)
        {
            var targetAssembly = Assembly.LoadFile(targetFileName);
            var controlTypeName = Path.GetFileNameWithoutExtension(resourceName);
            var controlType = targetAssembly.GetTypes().FirstOrDefault(type => type.Name.Equals(controlTypeName, StringComparison.OrdinalIgnoreCase));

            if (controlType == null)
                throw new InvalidOperationException($"The project does not contain a type named '{controlTypeName}'. Add a user control named {controlTypeName}.xaml as a template for your splash screen.");

            var dispatcher = Dispatcher.CurrentDispatcher;
     
            UIElement control = null;
            Stream bitmap = null;

            dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => control = CreateControl(controlType)));
            dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => bitmap = GenerateBitmap(control, target)));
            dispatcher.BeginInvokeShutdown(DispatcherPriority.ContextIdle);

            Dispatcher.Run();

            return bitmap;
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

        private static UIElement CreateControl(Type controlType)
        {
            try
            {
                return (UIElement)Activator.CreateInstance(controlType);
            }
            catch
            {
                throw new InvalidOperationException($"Type {controlType} is not a UIElement with a default constructor. You need to have a user control named {controlType.Name}.xaml as a template for your splash screen.");
            }
        }
    }
}