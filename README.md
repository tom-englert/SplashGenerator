# SplashGenerator
SplashGenerator is a MSBuild target that automates the generation of the WPF splash screen bitmap from a WPF control.

## How it works
Install the SplashGenerator tool from [NuGet](https://www.nuget.org/packages/SplashGenerator) to your WPF application that should have a splash screen.

**If you don't have a splash screen added to your application yet:**
- a sample splash image and user control will be added automatically to your project as a fully working scaffold you can start with immedeatly.

**If you already have a splash screen added to your application:**
- you need to add a new WPF UserControl to your project with the same name as the splash bitmap (e.g. Splash.png => Splash.xaml)
- do not forget to set `TextOptions.TextFormattingMode="Display"`, else text might look ugly. You can use the sample [Splash.xaml](https://github.com/tom-englert/SplashGenerator/blob/master/SplashGenerator/Splash.xaml) as a scaffold

When installing the SplashGenerator NuGet package a new build target will be added to your project. 
This new build target ensures that the splash bitmap will be updated from the WPF UserControl after each build.

Now you just need to design your splash UserControl. File and version info can be used dynamically via binding. However due to the fact that the final splash is a bitmap, animations are not supported.

To get more information how the WPF splash screen works see e.g. [How to: Add a Splash Screen to a WPF Application](https://msdn.microsoft.com/en-us/library/cc656886.aspx)






