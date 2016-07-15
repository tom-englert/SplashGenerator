namespace Sample
{
    using System;
    using System.Threading;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            Thread.Sleep(TimeSpan.FromSeconds(3));

            InitializeComponent();
        }
    }
}
