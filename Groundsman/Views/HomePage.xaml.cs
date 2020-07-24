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
    }
}
