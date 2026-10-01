using Dimitrova_3_bldg_3.ViewModel;

namespace Dimitrova_3_bldg_3;

public partial class RepairStatsPage : ContentPage
{
	public RepairStatsPage(RepairViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}