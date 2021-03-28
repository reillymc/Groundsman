using Groundsman.Models;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Groundsman.Interfaces
{
    public interface INavigationService<T>
    {
        Task NavigateToDetailPage(T feature);
        Task NavigateToEditPage(T feature);
        Task NavigateToNewEditPage(GeoJSONType type);
        Task PushAddFeaturePage();
        Task PushWelcomePage();
        Task NavigateBack(bool modal);
        Page GetCurrentPage();
        Task<bool> ShowAlert(string title, string body, bool question);
        Task ShowImportAlert(int successfulImports);
    }
}
