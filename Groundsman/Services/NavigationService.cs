using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Groundsman.Services
{
    public class NavigationService
    {
        public async Task NavigateToDetailPage(Feature feature)
        {
            var currentPage = GetCurrentPage();

            await currentPage.Navigation.PushAsync(new FeatureDetailsView(feature));
        }

        public async Task NavigateToEditPage(Feature feature)
        {
            var currentPage = GetCurrentPage();

            await currentPage.Navigation.PushAsync(new EditFeatureDetailsView(feature));
        }

        public async Task NavigateToNewEditPage(string type)
        {
            var currentPage = GetCurrentPage();

            await currentPage.Navigation.PushAsync(new EditFeatureDetailsView(type));
        }

        public async Task PushAddFeaturePage()
        {
            var currentPage = GetCurrentPage();
            await currentPage.Navigation.PushModalAsync(new AddFeatureView(true));
        }

        public async Task NavigateBack(bool modal)
        {
            var currentPage = GetCurrentPage();

            if (modal)
            {
                await currentPage.Navigation.PopModalAsync();
            }
            else
            {
                await currentPage.Navigation.PopAsync();
            }
        }

        public Page GetCurrentPage()
        {
            var currentPage = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();

            return currentPage;
        }
    }
}
