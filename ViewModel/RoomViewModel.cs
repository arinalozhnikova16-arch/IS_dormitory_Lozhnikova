using Dimitrova_3_bldg_3.Model;
using Dimitrova_3_bldg_3.Repository;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Microsoft.Maui.Devices.Sensors;

namespace Dimitrova_3_bldg_3.ViewModel
{
    public class RoomViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #region Поля
        private readonly DormitoryService _database;
        private int occupiedBeds = 0;
        private int totalBeds;
        private double area;
        private double floor;
        private string gender;
        private int section;
        private int number;
        private int id;
        #endregion

        #region Свойства фильтрации, сортировки и поиска
        public string SectionFilter { get; set; }
        public string GenderFilter { get; set; } = "Все";
        public string RoomSortOption { get; set; } = "По номеру комнаты";
        public string SearchQuery { get; set; }
        #endregion

        #region Диаграммы (Series и подписи)
        public ISeries[] OccupiedRoomsSeries { get; set; } = Array.Empty<ISeries>();
        public ISeries[] OccupiedBedsSeries { get; set; } = Array.Empty<ISeries>();
        public ISeries[] GenderDistributionSeries { get; set; } = Array.Empty<ISeries>();
        public ISeries[] SectionOccupancySeries { get; set; } = Array.Empty<ISeries>();

        public string[] RoomsLabels { get; set; } = { "Занято", "Свободно" };
        public string[] BedsLabels { get; set; } = { "Занято", "Свободно" };
        public string[] GenderLabels { get; set; } = { "Мужские", "Женские" };
        public string[] SectionLabels { get; set; } = Array.Empty<string>();
        public Axis[] SectionXAxes { get; set; } = Array.Empty<Axis>();
        #endregion

        #region Коллекция и свойства комнаты
        public ObservableCollection<Room> Rooms { get; set; } = [];
        public int Id
        {
            get => id;
            set
            {
                if (id == value) return;
                id = value;
                OnPropertyChanged();
            }
        } // Идентификатор комнаты
        public int Number
        {
            get => number;
            set
            {
                if (number == value) return;
                number = value;
                OnPropertyChanged();
            }
        } // Номер комнаты
        public int Section
        {
            get => section;
            set
            {
                if (section == value) return;
                section = value;
                OnPropertyChanged();
            }
        } // Номер секции
        public string Gender
        {
            get => gender;
            set
            {
                if (gender == value) return;
                gender = value;
                OnPropertyChanged();
            }
        } // Тип комнаты
        public double Floor
        {
            get => floor;
            set
            {
                if (floor == value) return;
                floor = value;
                OnPropertyChanged();
            }
        } // Номер этажа
        public double Area
        {
            get => area;
            set
            {
                if (area == value) return;
                area = value;
                OnPropertyChanged();
            }
        } // Площадь
        public int TotalBeds
        {
            get => totalBeds;
            set
            {
                if (totalBeds == value) return;
                totalBeds = value;
                OnPropertyChanged();
            }
        } // Общее кол-во койко-мест
        public int OccupiedBeds
        {
            get => occupiedBeds;
            set
            {
                if (occupiedBeds == value) return;
                occupiedBeds = value;
                OnPropertyChanged();
            }
        } // Кол-во занятых койко-мест
        #endregion

        #region ICommands
        public ICommand AddCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand SelectedItemCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand FilterCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand SortRoomsCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        #endregion

        public RoomViewModel(DormitoryService database)
        {
            _database = database;
            this.Rooms = new ObservableCollection<Room>(_database.GetRooms());

            #region Инициализация CRUD

            // Добавление
            AddCommand = new Command(async () =>
            {
                if (Number <= 0) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Укажите номер комнаты (>0)", "ОК"); return; }
                if (Section <= 0) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Укажите номер секции (>0)", "ОК"); return; }
                if (string.IsNullOrWhiteSpace(Gender)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите тип комнаты", "ОК"); return; }
                if (Floor <= 0) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Укажите этаж (>0)", "ОК"); return; }
                if (Area <= 0) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Укажите площадь (>0)", "ОК"); return; }
                if (TotalBeds <= 0) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Укажите количество койко-мест (>0)", "ОК"); return; }

                Room room = new()
                {
                    Number = this.Number,
                    Section = this.Section,
                    Gender = this.Gender,
                    Floor = this.Floor,
                    Area = this.Area,
                    TotalBeds = this.TotalBeds,
                    OccupiedBeds = this.OccupiedBeds,
                };
                Rooms.Add(room);
                _database.AddRoom(room);
                Vibration.Default.Vibrate(200);
                await Application.Current.MainPage.DisplayAlert("Успех", "Комната добавлена", "ОК");
                RefreshData();
                OnPropertyChanged(nameof(Rooms));
            });

            // Удаление
            RemoveCommand = new Command<Room>(async (room) =>
            {
                bool hasRelated = _database.GetResidents().Any(r => r.RoomId?.Id == room.Id) || _database.GetRepairs().Any(r => r.RoomId?.Id == room.Id);
                bool confirm = await Application.Current.MainPage.DisplayAlert("Внимание",
                    hasRelated ? $"Комната связана с проживающими/заявками. Удаление затронет все связанные данные. Продолжить?" : "Удалить эту комнату?", "Да", "Нет");
                if (!confirm) return;

                Rooms.Remove(room);
                _database.RemoveRoom(room);
                Vibration.Default.Vibrate(200);
                await Application.Current.MainPage.DisplayAlert("Успех", "Комната удалена", "ОК");
                RefreshData();
                OnPropertyChanged(nameof(Rooms));
            }, (room) => { return room != null; });

            // Обновление
            UpdateCommand = new Command<Room>(async (room) =>
            {
                if (Number <= 0) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Укажите номер комнаты (>0)", "ОК"); return; }
                if (Section <= 0) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Укажите номер секции (>0)", "ОК"); return; }
                if (string.IsNullOrWhiteSpace(Gender)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите тип комнаты", "ОК"); return; }
                if (Floor <= 0) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Укажите этаж (>0)", "ОК"); return; }
                if (Area <= 0) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Укажите площадь (>0)", "ОК"); return; }
                if (TotalBeds <= 0) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Укажите количество койко-мест (>0)", "ОК"); return; }

                int i = Rooms.IndexOf(room);
                room.Number = this.Number;
                room.Section = this.Section;
                room.Gender = this.Gender;
                room.Floor = this.Floor;
                room.Area = this.Area;
                room.TotalBeds = this.TotalBeds;
                room.OccupiedBeds = this.OccupiedBeds;
                Rooms[i] = room;
                _database.UpdateRoom(room);
                Vibration.Default.Vibrate(200);
                await Application.Current.MainPage.DisplayAlert("Успех", "Данные обновлены", "ОК");
                RefreshData();
                OnPropertyChanged(nameof(Rooms));
            }, (room) => { return room != null; });

            // Выбранный элемент
            SelectedItemCommand = new Command<Room>((room) =>
            {
                Number = room.Number;
                Section = room.Section;
                Gender = room.Gender;
                Floor = room.Floor;
                Area = room.Area;
                TotalBeds = room.TotalBeds;
                OccupiedBeds = room.OccupiedBeds;
            }, (person) => { return person != null; });
            #endregion

            #region Очистка полей, фильтрация, сортировки и поиск

            // Очистка полей формы
            ClearCommand = new Command(() =>
            {
                Id = 0;
                Number = 0;
                Section = 0;
                Gender = "Мужская";
                Floor = 0;
                Area = 0;
                TotalBeds = 0;
                OccupiedBeds = 0;
            });

            // Фильтрация
            FilterCommand = new Command(() => {
                var q = _database.GetRooms().AsEnumerable();
                if (int.TryParse(SectionFilter, out int s)) q = q.Where(r => r.Section == s);
                if (GenderFilter != "Все" && !string.IsNullOrWhiteSpace(GenderFilter))
                    q = q.Where(r => r.Gender == GenderFilter);
                Rooms = new ObservableCollection<Room>(q);
                OnPropertyChanged(nameof(Rooms));
            });

            // Очистка фильтарции и поиска
            ResetCommand = new Command(() => {
                SectionFilter = string.Empty; GenderFilter = "Все";
                Rooms = new ObservableCollection<Room>(_database.GetRooms());
                OnPropertyChanged(nameof(SectionFilter));
                OnPropertyChanged(nameof(GenderFilter));
                OnPropertyChanged(nameof(Rooms));
            });

            // Сортировка
            SortRoomsCommand = new Command(() => {
                var q = Rooms.AsEnumerable();
                q = RoomSortOption switch
                {
                    "По номеру комнаты" => q.OrderBy(r => r.Number),
                    "По этажу" => q.OrderBy(r => r.Floor),
                    "По занятым местам" => q.OrderBy(r => r.OccupiedBeds),
                    _ => q
                };
                Rooms = new ObservableCollection<Room>(q);
                OnPropertyChanged(nameof(Rooms));
            });

            // Поиск по номеру комнты
            SearchCommand = new Command(() => {
                var q = Rooms.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(SearchQuery) && int.TryParse(SearchQuery, out int num))
                    q = q.Where(r => r.Number == num);
                Rooms = new ObservableCollection<Room>(q);
                OnPropertyChanged(nameof(Rooms));
            });
            #endregion
        }

        #region Диаграммы
        private void CalculateStatistics()
        {
            var allRooms = _database.GetRooms().ToList();

            // 1. Занятые / свободные комнаты
            int fullyOccupiedRoomsCount = allRooms.Count(r => r.OccupiedBeds == r.TotalBeds);
            int partiallyOrFreeRoomsCount = allRooms.Count(r => r.OccupiedBeds < r.TotalBeds);

            OccupiedRoomsSeries = new ISeries[]
            {
                new PieSeries<int>
                {
                    Values = new[] { fullyOccupiedRoomsCount },
                    Name = "Полностью заселены",
                    Fill = new SolidColorPaint(SKColor.Parse("#001d37"))
                },
                new PieSeries<int>
                {
                    Values = new[] { partiallyOrFreeRoomsCount },
                    Name = "Есть свободные места",
                    Fill = new SolidColorPaint(SKColor.Parse("#0058a7"))
                }
            };

            // 2. Занятые / свободные койко-места
            int totalBeds = allRooms.Sum(r => r.TotalBeds);
            int occupiedBeds = allRooms.Sum(r => r.OccupiedBeds);
            int freeBeds = totalBeds - occupiedBeds;

            OccupiedBedsSeries = new ISeries[]
            {
                new PieSeries<int>
                {
                    Values = new[] { occupiedBeds },
                    Name = "Занято мест",
                    Fill = new SolidColorPaint(SKColor.Parse("#001d37"))
                },
                new PieSeries<int>
                {
                    Values = new[] { freeBeds },
                    Name = "Свободно мест",
                    Fill = new SolidColorPaint(SKColor.Parse("#0058a7"))
                }
            };

            // 3. Мужские / женские комнаты
            int maleRooms = allRooms.Count(r => r.Gender == "Мужская");
            int femaleRooms = allRooms.Count(r => r.Gender == "Женская");

            GenderDistributionSeries = new ISeries[]
            {
                new PieSeries<int>
                {
                    Values = new[] { maleRooms },
                    Name = "Мужские комнаты",
                    Fill = new SolidColorPaint(SKColor.Parse("#001d37"))
                },
                new PieSeries<int>
                {
                    Values = new[] { femaleRooms },
                    Name = "Женские комнаты",
                    Fill = new SolidColorPaint(SKColor.Parse("#0058a7"))
                }
            };

            // 4. Загруженность секций
            var sections = allRooms.GroupBy(r => r.Section).OrderBy(g => g.Key).ToList();

            var roomCountsPerSection = new List<int>();
            var residentCountsPerSection = new List<int>();
            var sectionNames = new List<string>();

            foreach (var section in sections)
            {
                int roomsInSection = section.Count();
                int residentsInSection = section.Sum(r => r.OccupiedBeds);

                roomCountsPerSection.Add(roomsInSection);
                residentCountsPerSection.Add(residentsInSection);
                sectionNames.Add($"Секция {section.Key}");
            };

            SectionXAxes = new Axis[]
{
                new Axis
                {
                    Labels = sectionNames.ToArray(),
                    LabelsRotation = 0,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray),
                    TicksPaint = new SolidColorPaint(SKColors.Black)
                }
            };

            SectionOccupancySeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = roomCountsPerSection.ToArray(),
                    Name = "Кол-во комнат",
                    Fill = new SolidColorPaint(SKColor.Parse("#001d37"))
                },
                new ColumnSeries<int>
                {
                    Values = residentCountsPerSection.ToArray(),
                    Name = "Кол-во проживающих",
                    Fill = new SolidColorPaint(SKColor.Parse("#0058a7"))
                }
            };

            OnPropertyChanged(nameof(OccupiedRoomsSeries));
            OnPropertyChanged(nameof(OccupiedBedsSeries));
            OnPropertyChanged(nameof(GenderDistributionSeries));
            OnPropertyChanged(nameof(SectionOccupancySeries));
            OnPropertyChanged(nameof(SectionXAxes));
        }
        #endregion

        #region Обновление данных
        public void RefreshData()
        {
            Rooms = new ObservableCollection<Room>(_database.GetRooms());
            OnPropertyChanged(nameof(Rooms));

            CalculateStatistics();

            OnPropertyChanged(nameof(OccupiedRoomsSeries));
            OnPropertyChanged(nameof(OccupiedBedsSeries));
            OnPropertyChanged(nameof(GenderDistributionSeries));
            OnPropertyChanged(nameof(SectionOccupancySeries));
            OnPropertyChanged(nameof(SectionXAxes));
        }
        #endregion
    }
}
