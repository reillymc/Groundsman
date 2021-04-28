using Xamarin.Forms;

namespace Groundsman.Controls
{
    public partial class ModalNavigationBar : ContentView
    {
        public static readonly BindableProperty HasDoneButtonProperty = BindableProperty.Create(nameof(HasDoneButton), typeof(bool), typeof(ModalNavigationBar), false);

        public bool HasDoneButton
        {
            get => (bool)GetValue(HasDoneButtonProperty);
            set => SetValue(HasDoneButtonProperty, value);
        }

        public ModalNavigationBar() => InitializeComponent();
    }
}
