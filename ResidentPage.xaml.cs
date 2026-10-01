using Dimitrova_3_bldg_3.Model;
using Dimitrova_3_bldg_3.Repository;
using Dimitrova_3_bldg_3.ViewModel;

namespace Dimitrova_3_bldg_3;

public partial class ResidentPage : ContentPage
{
    readonly ResidentViewModel _viewModel;
    public ResidentPage(ResidentViewModel viewModel)
	{
        InitializeComponent();
        this.BindingContext = _viewModel = viewModel;
    }

    private async void OnShowStatsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("residentStatsRoute");
    }

    private async void OnSelectRoomClicked(object sender, EventArgs e)
    {
        var db = Application.Current.Handler.MauiContext.Services.GetRequiredService<DormitoryService>();
        var page = new SelectRoomPage(db, selectedRoom =>
        {
            _viewModel.RoomId = selectedRoom;
        });
        await Navigation.PushModalAsync(page);
    }

    private void cvResidents_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            Resident resident = e.CurrentSelection[0] as Resident;
            entFistName.Text = _viewModel.FirstName = resident.FirstName;
            entLastName.Text = _viewModel.LastName = resident.LastName;
            entMiddleName.Text = _viewModel.MiddleName = resident.MiddleName;

            _viewModel.RoomId = resident.RoomId;

            pickerFaculty.SelectedItem = _viewModel.Faculty = resident.Faculty;

            entCourse.Text = resident.Course.ToString();
            _viewModel.Course = resident.Course;

            dateDateOfBirth.Date = _viewModel.DateOfBirth = resident.DateOfBirth;

            entPhone.Text = _viewModel.Phone = resident.Phone;
            entEmail.Text = _viewModel.Email = resident.Email;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshData();
    }
}