using Dimitrova_3_bldg_3.ViewModel;

namespace Dimitrova_3_bldg_3;

public partial class ResidentStatsPage : ContentPage
{
	public ResidentStatsPage(ResidentViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}