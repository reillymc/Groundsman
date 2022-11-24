using Groundsman.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Groundsman.ViewModels;

[QueryProperty("Feature", "Feature")]
public partial class EditViewModel : BaseViewModel
{
    public EditViewModel()
    {

    }

    [ObservableProperty]
    Feature feature;
}

