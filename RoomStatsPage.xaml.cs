using Dimitrova_3_bldg_3.ViewModel;

namespace Dimitrova_3_bldg_3;

public partial class RoomStatsPage : ContentPage
{
	public RoomStatsPage(RoomViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}