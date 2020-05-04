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
        public async Task ShowNewDetailFormPage(string type)
        {
            await Navigation.PushAsync(new EditFeatureDetailsView(type));
        }

        /// <summary>
        /// Shows the edit feature detials page for an existing feature
        /// </summary>
        /// <param name="feature"></param>
        public async Task ShowEditDetailFormPage(Feature feature)
        {
            await Navigation.PushAsync(new EditFeatureDetailsView(feature));
        }

        /// <summary>
        /// Shows the feature details page for an existing feature
        /// </summary>
        /// <param name="feature">Feature to get and display data from</param>
        /// <returns></returns>
        public async Task ShowDetailFormPage(Feature feature)
        {
            await Navigation.PushAsync(new FeatureDetailsView(feature));
        }

        /// <summary>
        /// Displays native device share sheet to export feature list
        /// </summary>
        /// <returns></returns>
        public async Task ShowShareSheetAsync()
        {
            await App.FeatureStore.ExportFeatures();
        }

        /// <summary>
        /// Displays a pop-up user interface to navigate to different data entry types
        /// </summary>
        /// <returns></returns>
        public async Task ShowAddFeaturePage()
        {
            await Navigation.PushModalAsync(new AddFeatureView());
        }
    }
}
