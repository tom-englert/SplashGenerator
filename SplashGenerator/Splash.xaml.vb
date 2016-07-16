Public Class Splash

    Public Sub New()

        FileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)

        InitializeComponent()

    End Sub

    Public ReadOnly Property FileVersionInfo As System.Diagnostics.FileVersionInfo

End Class
