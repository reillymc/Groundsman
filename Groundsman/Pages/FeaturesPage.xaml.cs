using Groundsman.ViewModels;

namespace Groundsman.Pages;

public partial class FeaturesPage : ContentPage
{
    public FeaturesPage(FeaturesViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}