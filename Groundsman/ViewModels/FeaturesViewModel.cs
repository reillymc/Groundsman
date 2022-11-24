using System.Collections.ObjectModel;
using Groundsman.Helpers;
using Groundsman.Models;
using Groundsman.Pages;
using Groundsman.Services;
using Microsoft.Toolkit.Mvvm.Input;

namespace Groundsman.ViewModels;

public partial class FeaturesViewModel : BaseViewModel
{
    private readonly FeatureService featureService;

    public ObservableCollection<Feature> Features { get; } = new();

    public string FeatureCount { get => $"{Features.Count} Feature{(Features.Count == 1 ? "" : "s")}"; }

    public FeaturesViewModel(FeatureService featureService)
    {
        this.featureService = featureService;
        _ = GetFeatures();
    }

    private async Task GetFeatures()
    {
        if (IsBusy)
            return;


        IsBusy = true;
        var features = await featureService.GetFeatures();

        Features.Clear();

        Features.Add(DefaultFeatures.DefaultPoint);
        Features.Add(DefaultFeatures.DefaultLine);
        Features.Add(DefaultFeatures.DefaultPolygon);


        foreach (var feature in features)
            Features.Add(feature);

        IsBusy = false;
    }

    [ICommand]
    private async Task EditFeature(Feature feature)
    {
        if (feature is null) return;

        await Shell.Current.GoToAsync(nameof(EditPage), new Dictionary<string, object>
        {
            { "Feature", feature },
        });
    }

    [ICommand]
    private async Task AddFeature()
    {
        await Shell.Current.GoToAsync(nameof(AddPage));
    }

    [ICommand]
    private async Task ShareFeatures()
    {
        // TODO: Share features
    }

    [ICommand]
    private async Task ShareFeature(Feature feature)
    {
        // TODO: Share feature
    }

    [ICommand]
    private async Task DeleteFeature(Feature feature)
    {
        // TODO: Delete feature
    }
}