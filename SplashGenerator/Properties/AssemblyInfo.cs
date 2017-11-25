using System.Reflection;

using SplashGenerator.Properties;

[assembly: AssemblyTitle("SplashGenerator")]
[assembly: AssemblyDescription("SplashGenerator is a MSBuild target that automates the generation of the WPF splash screen bitmap from a WPF control.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("tom-englert.de")]
[assembly: AssemblyProduct("Tom's Toolbox")]
[assembly: AssemblyCopyright("Copyright © tom-englert.de 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion(Product.Version)]
[assembly: AssemblyFileVersion(Product.Version)]

namespace SplashGenerator.Properties
{
    internal class Product
    {
        public const string Version = "1.0.2.0";
    }
}
