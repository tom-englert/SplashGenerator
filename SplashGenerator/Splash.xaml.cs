using System.Diagnostics;
using System.Reflection;

/// <summary>
/// Interaction logic for Splash.xaml
/// </summary>
public partial class Splash
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Splash"/> class.
    /// </summary>
    public Splash()
    {
        FileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        InitializeComponent();
    }

    /// <summary>
    /// Gets the file description.
    /// </summary>
    public FileVersionInfo FileVersionInfo { get; private set; }
}

