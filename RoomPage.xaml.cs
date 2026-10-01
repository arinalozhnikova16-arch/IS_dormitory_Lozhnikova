using Dimitrova_3_bldg_3.Model;
using Dimitrova_3_bldg_3.ViewModel;

namespace Dimitrova_3_bldg_3;

public partial class RoomPage : ContentPage
{
    readonly RoomViewModel _viewModel;
    public RoomPage(RoomViewModel viewModel)
	{
        InitializeComponent();
        this.BindingContext = _viewModel = viewModel;
    }

    private async void OnShowStatsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("roomStatsRoute");
    }

    private void OnGenderChecked(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value && sender is RadioButton rb)
        {
            _viewModel.Gender = rb.Value.ToString();
        }
    }

    private void cvRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            Room room = e.CurrentSelection[0] as Room;
            entNumber.Text = room.Number.ToString();
            _viewModel.Number = room.Number;

            entSection.Text = room.Section.ToString();
            _viewModel.Section = room.Section;

            _viewModel.Gender = room.Gender ?? "Мужская";
            radioMale.IsChecked = _viewModel.Gender == "Мужская";
            radioFemale.IsChecked = _viewModel.Gender == "Женская";

            entFloor.Text = room.Floor.ToString();
            _viewModel.Floor = room.Floor;

            entArea.Text = room.Area.ToString();
            _viewModel.Area = room.Area;

            entTotalBeds.Text = room.TotalBeds.ToString();
            _viewModel.TotalBeds = room.TotalBeds;

            lblOccupiedBeds.Text = room.OccupiedBeds.ToString();
            _viewModel.OccupiedBeds = room.OccupiedBeds;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshData();
    }
}