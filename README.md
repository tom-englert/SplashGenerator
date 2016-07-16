# SplashGenerator
SplashGenerator is a MSBuild target that automates the generation of the WPF splash screen bitmap from a WPF control.

## How it works
Install the SplashGenerator tool from [NuGet](https://www.nuget.org/packages/SplashGenerator) to your WPF application that should have a splash screen.

###If you don't have a splash screen added to your application yet:
- a sample will be added automatically to your project as a scaffold you can start with immedeatly.

### If you already have a splash screen added to your application:
- you need to add a new WPF UserControl to your project with the same name as the splash bitmap (e.g. Splash.png => Splash.xaml)

When installing the SplashGenerator NuGet package a new build target will be added to your project. 
This new build target ensures that the splash bitmap will be updated from the WPF UserControl after each build. 

To get more infomrmation how the WPF splash screen works see e.g. [How to: Add a Splash Screen to a WPF Application](https://msdn.microsoft.com/en-us/library/cc656886.aspx)






