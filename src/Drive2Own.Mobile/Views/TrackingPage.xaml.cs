using Drive2Own.Mobile.ViewModels;

namespace Drive2Own.Mobile.Views;

public partial class TrackingPage : ContentPage
{
    public TrackingPage(TrackingViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
