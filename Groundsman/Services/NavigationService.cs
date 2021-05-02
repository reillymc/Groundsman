using System.Linq;
using System.Threading.Tasks;
using Groundsman.Interfaces;
using Groundsman.Models;
using Groundsman.ViewModels;
using Groundsman.Views;
using Xamarin.Forms;

namespace Groundsman.Services
{
    public class NavigationService : INavigationService<Feature>
    {
        public async Task NavigateToEditPage(Feature feature)
        {
            Page currentPage = GetCurrentPage();

            await currentPage.Navigation.PushModalAsync(new EditFeatureDetailsView(new FeatureDetailsViewModel(feature)));
        }

        public async Task NavigateToNewEditPage(GeoJSONType type)
        {
            Page currentPage = GetCurrentPage();

            await currentPage.Navigation.PushModalAsync(new EditFeatureDetailsView(new FeatureDetailsViewModel(type)));
        }

        public async Task NavigateToLoggerPage(Feature feature)
        {
            Page currentPage = GetCurrentPage();

            await currentPage.Navigation.PushModalAsync(new EditFeatureDetailsView(new LoggerViewModel(feature)));
        }

        public async Task NavigateToNewLoggerPage()
        {
            Page currentPage = GetCurrentPage();

            await currentPage.Navigation.PushModalAsync(new EditFeatureDetailsView(new LoggerViewModel()));
        }

        public async Task PushAddFeaturePage()
        {
            Page currentPage = GetCurrentPage();
            await currentPage.Navigation.PushModalAsync(new AddFeatureView());
        }

        public async Task PushWelcomePage()
        {
            Page currentPage = GetCurrentPage();
            await currentPage.Navigation.PushModalAsync(new WelcomeFormView());
        }

        public async Task NavigateBack(bool modal)
        {
            Page currentPage = GetCurrentPage();

            if (modal)
            {
                if (currentPage.Navigation.ModalStack.Count > 0)
                {
                    await currentPage.Navigation.PopModalAsync();
                }
            }
            else
            {
                if (currentPage.Navigation.NavigationStack.Count > 0)
                {
                    await currentPage.Navigation.PopAsync();
                }
            }
        }

        public Page GetCurrentPage()
        {
            Page currentPage = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();

            return currentPage;
        }

        public async Task<bool> ShowAlert(string title, string body, bool question)
        {
            Page currentPage = GetCurrentPage();
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

        public async Task ShowImportAlert(int successfulImports)
        {
            Page currentPage = GetCurrentPage();
            await currentPage.DisplayAlert("Feature Import", $"Groundsman has successfully imported {successfulImports} features!", "Ok");
        }
    }
}
