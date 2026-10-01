using Dimitrova_3_bldg_3.Model;
using Dimitrova_3_bldg_3.Repository;
using System.Collections.ObjectModel;

namespace Dimitrova_3_bldg_3;

public partial class SelectRoomPage : ContentPage
{
    private readonly DormitoryService _db;
    private readonly Action<Room> _onSelected;
    private Room _selectedRoom;
    private List<Room> _allRooms = new();
    public SelectRoomPage(DormitoryService db, Action<Room> onSelected)
	{
        _db = db;
        _onSelected = onSelected;
        InitializeComponent();

        _allRooms = _db.GetRooms().ToList();

        pickerGenderFilter.SelectedIndex = 0;
        ApplyRoomFilter();

        cvResidents.ItemsSource = new ObservableCollection<Resident>();
    }

    private void ApplyRoomFilter()
    {
        if (pickerGenderFilter.SelectedItem == null) return;

        string gender = pickerGenderFilter.SelectedItem.ToString();

        var query = _allRooms.Where(r => r.TotalBeds > 0 && r.OccupiedBeds < r.TotalBeds);

        if (gender == "Мужская")
            query = query.Where(r => r.Gender == "Мужская");
        else if (gender == "Женская")
            query = query.Where(r => r.Gender == "Женская");

        cvRooms.ItemsSource = new ObservableCollection<Room>(query);
    }
    private void OnGenderFilterChanged(object sender, EventArgs e)
    {
        ApplyRoomFilter();

        _selectedRoom = null;
        cvRooms.SelectedItem = null;
        cvResidents.ItemsSource = new ObservableCollection<Resident>();
    }

    private void OnRoomSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Room room)
        {
            _selectedRoom = room;
            var residents = _db.GetResidents().Where(r => r.RoomId?.Id == room.Id).ToList();
            cvResidents.ItemsSource = new ObservableCollection<Resident>(residents);
        }
    }

    private async void OnSelectClicked(object sender, EventArgs e)
    {
        if (_selectedRoom != null)
            _onSelected?.Invoke(_selectedRoom);

        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}