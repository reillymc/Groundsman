using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

// Needed to use C#9 outside of .NET 5
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}