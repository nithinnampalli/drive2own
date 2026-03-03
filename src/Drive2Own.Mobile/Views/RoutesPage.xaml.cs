using Drive2Own.Mobile.ViewModels;

namespace Drive2Own.Mobile.Views;

public partial class RoutesPage : ContentPage
{
    private readonly RoutesViewModel _vm;

    public RoutesPage(RoutesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadRoutesCommand.Execute(null);
    }
}
