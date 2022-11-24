using Groundsman.ViewModels;

namespace Groundsman.Pages;

public partial class EditPage : ContentPage
{
    public EditPage(EditViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}