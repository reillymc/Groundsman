using System.Threading.Tasks;
using Xamarin.Forms;

namespace Groundsman
{
    public partial class HomePage : TabbedPage
    {
        private static HomePage instance;
        public static HomePage Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HomePage();
                }
                return instance;
            }
        }

        public HomePage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Asynchronously adds DetailFormView to the top of the navigation stack.
        /// </summary>
        /// <param name="type">Data entry type</param>
        public void ShowNewDetailFormPage(string type)
        {
            Navigation.PushAsync(new EditFeatureDetailsView(type));
        }

        public void ShowEditDetailFormPage(Feature entryToEdit)
        {
            Navigation.PushAsync(new EditFeatureDetailsView(entryToEdit));
        }

        public async Task ShowExistingDetailFormPage(Feature data)
        {
            await Navigation.PushAsync(new FeatureDetailsView(data));
        }

        public async Task ShowShareSheetAsync()
        {
            await App.FeatureStore.ExportFeatures();
        }

        /// <summary>
        /// Displays a pop-up user interface to navigate to different data entry types
        /// </summary>
        /// <returns></returns>
        public async Task ShowDetailFormOptions()
        {
            await Navigation.PushModalAsync(new AddFeatureView());
        }
    }
}
