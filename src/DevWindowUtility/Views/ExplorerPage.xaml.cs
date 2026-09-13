using System.Windows.Controls;
using System.Windows.Input;
using DevWindowUtility.ViewModels;

namespace DevWindowUtility.Views;

public partial class ExplorerPage : UserControl
{
    public ExplorerPage()
    {
        InitializeComponent();
    }

    private void Favorites_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ExplorerViewModel viewModel)
        {
            viewModel.OpenFavoriteCommand.Execute(null);
        }
    }

    private void Entries_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ExplorerViewModel viewModel)
        {
            viewModel.OpenSelectedCommand.Execute(null);
        }
    }
}
