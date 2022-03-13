using Groundsman.ViewModels;
using Xamarin.Forms;

namespace Groundsman.Views
{	
	public partial class AboutView : ContentPage
	{
		private readonly AboutViewModel viewModel;

		public AboutView()
		{
			InitializeComponent();
			BindingContext = viewModel = new AboutViewModel();
		}
	}
}

