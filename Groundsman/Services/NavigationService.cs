using Groundsman.Interfaces;
using Groundsman.Models;
using Groundsman.Views;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Groundsman.Services
{
    public class NavigationService : INavigationService<Feature>
    {
        public async Task NavigateToDetailPage(Feature feature)
        {
            var currentPage = GetCurrentPage();

            await currentPage.Navigation.PushAsync(new FeatureDetailsView(feature));
        }

        public async Task NavigateToEditPage(Feature feature)
        {
            var currentPage = GetCurrentPage();

            await currentPage.Navigation.PushModalAsync(new EditFeatureDetailsView(feature));
        }

        public async Task NavigateToNewEditPage(GeoJSONType type)
        {
            var currentPage = GetCurrentPage();

            await currentPage.Navigation.PushModalAsync(new EditFeatureDetailsView(type));
        }

        public async Task PushAddFeaturePage()
        {
            var currentPage = GetCurrentPage();
            await currentPage.Navigation.PushModalAsync(new AddFeatureView());
        }

        public async Task PushWelcomePage()
        {
            var currentPage = GetCurrentPage();
            await currentPage.Navigation.PushModalAsync(new WelcomeFormView());
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

        public async Task<bool> ShowAlert(string title, string body, bool question)
        {
            var currentPage = GetCurrentPage();
            bool response = false;

            if (question)
            {
                response = await currentPage.DisplayAlert(title, body, "Yes", "No");
            }
            else
            {
                await currentPage.DisplayAlert(title, body, "Ok");
            }
            return response;
        }
    }
}
