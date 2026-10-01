using Dimitrova_3_bldg_3.Model;
using Dimitrova_3_bldg_3.ViewModel;

namespace Dimitrova_3_bldg_3;

public partial class RepairPage : ContentPage
{
    readonly RepairViewModel _viewModel;
    public RepairPage(RepairViewModel viewModel)
	{
        InitializeComponent();
        this.BindingContext = _viewModel = viewModel;
    }

    private async void OnShowStatsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("repairStatsRoute");
    }

    private void cvRepairs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            Repair repair = e.CurrentSelection[0] as Repair;
            entDescription.Text = _viewModel.Description = repair.Description;

            _viewModel.RoomId = repair.RoomId;
            pickerRoom.SelectedItem = repair.RoomId;

            _viewModel.ResidentId = repair.ResidentId;
            pickerResident.SelectedItem = repair.ResidentId;

            dateRequestDate.Date = _viewModel.RequestDate = repair.RequestDate;
            pickerStatus.SelectedItem = _viewModel.Status = repair.Status;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshData();
    }
}