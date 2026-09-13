using System.Windows;
using DevWindowUtility.ViewModels;

namespace DevWindowUtility;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
