using Dimitrova_3_bldg_3.Model;
using Dimitrova_3_bldg_3.Repository;
using LiveChartsCore;
using LiveChartsCore.Defaults;
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
    public class RepairViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #region Поля
        private readonly DormitoryService _database;
        private string status;
        private DateTime requestDate;
        private Resident residentId;
        private Room roomId;
        private string description;
        private int id;
        #endregion

        #region Свойство фильтрации, сортировки и поиска
        public string StatusFilter { get; set; } = "Все";
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string RepairSortOption { get; set; } = "По дате (убыв.)";
        public string SearchQuery { get; set; }
        #endregion

        #region Диаграммы (Series и подписи)
        public ISeries[] StatusDistributionSeries { get; set; } = Array.Empty<ISeries>();
        public ISeries[] RoomRequestCountSeries { get; set; } = Array.Empty<ISeries>();
        public ISeries[] DateTrendSeries { get; set; } = Array.Empty<ISeries>();

        public string[] StatusLabels { get; set; } = Array.Empty<string>();
        public string[] RoomLabels { get; set; } = Array.Empty<string>();
        public Axis[] RoomRequestXAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] DateXAxes { get; set; } = Array.Empty<Axis>();
        #endregion

        #region Коллекции и свойства заявки
        public ObservableCollection<Repair> Repairs { get; set; } = [];

        private ObservableCollection<Room> _roomList = new ObservableCollection<Room>();
        private ObservableCollection<Resident> _residentList = new ObservableCollection<Resident>();

        public ObservableCollection<Room> RoomList
        {
            get => _roomList;
            set
            {
                _roomList = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Resident> ResidentList
        {
            get => _residentList;
            set
            {
                _residentList = value;
                OnPropertyChanged();
            }
        }

        public int Id
        {
            get => id;
            set
            {
                if (id == value) return;
                id = value;
                OnPropertyChanged();
            }
        } // Идентификатор заявки
        public string Description
        {
            get => description;
            set
            {
                if (description == value) return;
                description = value;
                OnPropertyChanged();
            }
        } // Описание неисправности
        public Room RoomId
        {
            get => roomId;
            set
            {
                if (roomId == value) return;
                roomId = value;
                OnPropertyChanged();
                ApplyResidentFilter();
            }
        } // ИД команты
        public Resident ResidentId
        {
            get => residentId;
            set
            {
                if (residentId == value) return;
                residentId = value;
                OnPropertyChanged();
            }
        } // ИД проживаюшего
        public DateTime RequestDate
        {
            get => requestDate;
            set
            {
                if (requestDate == value) return;
                requestDate = value;
                OnPropertyChanged();
            }
        } // Дата подачи заявления
        public string Status
        {
            get => status;
            set
            {
                if (status == value) return;
                status = value;
                OnPropertyChanged();
            }
        } // Статус заявки
        #endregion

        #region Мастер список и свойства для автофильтрации "История текущего пользователя"
        private ObservableCollection<Repair> _masterRepairs = new();
        private bool _isFiltered = false;
        private string _filterButtonText = "Показать историю текущего пользователя";
        private ObservableCollection<Resident> _filteredResidentList = new ObservableCollection<Resident>();
        public string FilterButtonText
        {
            get => _filterButtonText;
            set { _filterButtonText = value;
                OnPropertyChanged(); }
        }
        public ObservableCollection<Resident> FilteredResidentList
        {
            get => _filteredResidentList;
            set
            {
                _filteredResidentList = value;
                OnPropertyChanged();

            }
        }
        #endregion

        #region ICommands
        public ICommand AddCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand SelectedItemCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand ToggleFilterCommand { get; set; }
        public ICommand FilterCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand SortRepairsCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        #endregion

        public RepairViewModel(DormitoryService database)
        {
            _database = database;
            _masterRepairs = new ObservableCollection<Repair>(_database.GetRepairs());
            Repairs = new ObservableCollection<Repair>(_masterRepairs);

            this.RoomList = new ObservableCollection<Room>(_database.GetRooms());
            this.ResidentList = new ObservableCollection<Resident>(_database.GetResidents());

            this.RequestDate = DateTime.Today;
            this.Status = "На рассмотрении";

            #region Инициализация автофильтра
            ToggleFilterCommand = new Command(async () =>
            {
                if (!_isFiltered)
                {
                    if (ResidentId == null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Внимание", "Сначала выберите проживающего в форме!", "ОК");
                        return;
                    }
                    var filtered = _masterRepairs.Where(r => r.ResidentId?.Id == ResidentId.Id).ToList();
                    Repairs.Clear();
                    foreach (var r in filtered) Repairs.Add(r);
                    _isFiltered = true;
                    FilterButtonText = "Показать всю историю";
                }
                else
                {
                    Repairs.Clear();
                    foreach (var r in _masterRepairs) Repairs.Add(r);
                    _isFiltered = false;
                    FilterButtonText = "Показать историю текущего пользователя";
                }
                OnPropertyChanged(nameof(FilterButtonText));
            });
            #endregion

            #region Инициализация CRUD

            // Добавление
            AddCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(Description)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите описание неисправности", "ОК"); return; }
                if (RoomId == null) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите комнату", "ОК"); return; }
                if (ResidentId == null) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите подавшего заявку", "ОК"); return; }

                Repair repair = new()
                {
                    Description = this.Description,
                    RoomId = this.RoomId,
                    ResidentId = this.ResidentId,
                    RequestDate = this.RequestDate,
                    Status = this.Status,
                };
                Repairs.Add(repair);    
                _masterRepairs.Add(repair);
                _database.AddRepair(repair);

                if (ResidentId != null)
                {
                    var residentRepairs = _masterRepairs.Where(r => r.ResidentId?.Id == ResidentId.Id).ToList();
                    Repairs.Clear();
                    foreach (var r in residentRepairs) Repairs.Add(r);
                    _isFiltered = true;
                    FilterButtonText = "Показать всю историю";
                    OnPropertyChanged(nameof(FilterButtonText));
                }
                Vibration.Default.Vibrate(200);
                await Application.Current.MainPage.DisplayAlert("Успех", "Заявка добавлена", "ОК");
                RefreshData();
                OnPropertyChanged(nameof(Repairs));
            });

            // Удаление
            RemoveCommand = new Command<Repair>(async (repair) =>
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert("Удаление", "Удалить заявку?", "Да", "Нет");
                if (!confirm) return;
                Repairs.Remove(repair);
                _masterRepairs.Remove(repair);
                _database.RemoveRepair(repair);
                Vibration.Default.Vibrate(200);
                await Application.Current.MainPage.DisplayAlert("Успех", "Заявка удалена", "ОК");
                RefreshData();
                OnPropertyChanged(nameof(Repairs));
            }, (repair) => { return repair != null; });

            // Обновление
            UpdateCommand = new Command<Repair>(async (repair) =>
            {
                if (string.IsNullOrWhiteSpace(Description)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите описание неисправности", "ОК"); return; }
                if (RoomId == null) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите комнату", "ОК"); return; }
                if (ResidentId == null) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите подавшего заявку", "ОК"); return; }

                int i = Repairs.IndexOf(repair);
                repair.Description = this.Description;
                repair.RoomId = this.RoomId;
                repair.ResidentId = this.ResidentId;
                repair.RequestDate = this.RequestDate;
                repair.Status = this.Status;
                Repairs[i] = repair;
                _database.UpdateRepair(repair);
                Vibration.Default.Vibrate(200);
                await Application.Current.MainPage.DisplayAlert("Успех", "Заявка обновлена", "ОК");
                RefreshData();
                OnPropertyChanged(nameof(Repairs));
            }, (repair) => { return repair != null; });

            // Выбранный элемент
            SelectedItemCommand = new Command<Repair>((repair) =>
            {
                Description = repair.Description;
                RoomId = repair.RoomId;
                ResidentId = repair.ResidentId;
                RequestDate = repair.RequestDate;
                Status = repair.Status;
            }, (repair) => { return repair != null; });
            #endregion

            #region Очистка полей, фильтрация, сортировка и поиск

            // Очистка полей формы
            ClearCommand = new Command(() =>
            {
                Id = 0;
                Description = string.Empty;
                RoomId = null;
                ResidentId = null;
                RequestDate = DateTime.Today;
                Status = "На рассмотрении";
            });

            // Фильтраиця
            FilterCommand = new Command(() => {
                var q = _masterRepairs.AsEnumerable();
                if (StatusFilter != "Все") q = q.Where(r => r.Status == StatusFilter);
                if (DateFrom.HasValue) q = q.Where(r => r.RequestDate >= DateFrom.Value.Date);
                if (DateTo.HasValue) q = q.Where(r => r.RequestDate <= DateTo.Value.Date.AddDays(1).AddTicks(-1));
                Repairs = new ObservableCollection<Repair>(q);
                OnPropertyChanged(nameof(Repairs));
            });

            // Очитска фильтрации и поиска
            ResetCommand = new Command(() => {
                StatusFilter = "Все"; DateFrom = null; DateTo = null;
                Repairs = new ObservableCollection<Repair>(_masterRepairs);
                OnPropertyChanged(nameof(StatusFilter));
                OnPropertyChanged(nameof(DateFrom));
                OnPropertyChanged(nameof(DateTo));
                OnPropertyChanged(nameof(Repairs));
            });

            // Сортировка
            SortRepairsCommand = new Command(() => {
                var q = Repairs.AsEnumerable();
                q = RepairSortOption switch {
                    "По дате (возр.)" => q.OrderBy(r => r.RequestDate),
                    "По дате (убыв.)" => q.OrderByDescending(r => r.RequestDate),
                    _ => q
                };
                Repairs = new ObservableCollection<Repair>(q);
                OnPropertyChanged(nameof(Repairs));
            });

            // Поиск по номеру комнаты
            SearchCommand = new Command(() => {
                var q = Repairs.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(SearchQuery) && int.TryParse(SearchQuery, out int roomNum))
                    q = q.Where(r => r.RoomId?.Number == roomNum);
                Repairs = new ObservableCollection<Repair>(q);
                OnPropertyChanged(nameof(Repairs));
            });
            #endregion
        }

        #region Применение автофильтра
        private void ApplyResidentFilter()
        {
            var filtred = RoomId != null
                ? ResidentList?.Where(r => r.RoomId == RoomId).ToList()
                : ResidentList?.ToList();
            FilteredResidentList = new ObservableCollection<Resident>(filtred ?? new List<Resident>());
        }
        #endregion

        #region Диаграммы
        private void CalculateRepairStatistics()
        {
            var allRepairs = _database.GetRepairs().ToList();

            // 1. Соотношение статусов
            var statusGroups = allRepairs.GroupBy(r => r.Status)
                                         .OrderBy(g => g.Key)
                                         .ToList();
            int status1 = allRepairs.Count(r => r.Status == "На рассмотрении");
            int status2 = allRepairs.Count(r => r.Status == "Принята");
            int status3 = allRepairs.Count(r => r.Status == "Выполнена");
            int status4 = allRepairs.Count(r => r.Status == "Отклонена");

            StatusDistributionSeries = new ISeries[]
            {
                new PieSeries<int>
                {
                    Values = new[] { status1 },
                    Name = "Заявок на рассмотрении",
                    Fill = new SolidColorPaint(SKColor.Parse("#2196ff"))
                },
                new PieSeries<int>
                {
                    Values = new[] { status2 },
                    Name = "Заявок принято",
                    Fill = new SolidColorPaint(SKColor.Parse("#0072d9"))
                },
                new PieSeries<int>
                {
                    Values = new[] { status3 },
                    Name = "Заявок выполнено",
                    Fill = new SolidColorPaint(SKColor.Parse("#0058a7"))
                },
                new PieSeries<int>
                {
                    Values = new[] { status4 },
                    Name = "Заявок отклонено",
                    Fill = new SolidColorPaint(SKColor.Parse("#001d37"))
                }
            };

            // 2. Заявки по комнатам
            var roomGroups = allRepairs.GroupBy(r => r.RoomId?.Number ?? 0)
                                       .Where(g => g.Key != 0)
                                       .OrderBy(g => g.Key)
                                       .ToList();

            var roomValues = new List<int>();
            var roomNumbers = new List<string>();

            foreach (var group in roomGroups)
            {
                roomValues.Add(group.Count());
                roomNumbers.Add($"Комн. {group.Key}");
            }

            RoomRequestXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = roomNumbers.ToArray(),
                    LabelsRotation = -45,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray),
                    TicksPaint = new SolidColorPaint(SKColors.Black)
                }
            };

            RoomRequestCountSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = roomValues.ToArray(),
                    Name = "Кол-во заявок",
                    Fill = new SolidColorPaint(SKColor.Parse("#0058a7"))
                }
            };

            RoomLabels = roomNumbers.ToArray();

            // 3. Динамика по датам
            var dateGroups = allRepairs.GroupBy(r => r.RequestDate.Date)
                                       .OrderBy(g => g.Key)
                                       .ToList();

            var datePoints = new List<ObservableValue>();
            var dateLabels = new List<string>();

            for (int i = 0; i < dateGroups.Count; i++)
            {
                datePoints.Add(new ObservableValue(dateGroups[i].Count()));
                dateLabels.Add(dateGroups[i].Key.ToString("dd.MM.yyyy"));
            }

            DateXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = dateLabels.ToArray(),
                    LabelsRotation = 45
                }
            };

            DateTrendSeries = new ISeries[]
            {
                new LineSeries<ObservableValue>
                {
                    Values = datePoints,
                    Name = "Количество заявок",
                    Fill = new SolidColorPaint(SKColor.Parse("#0072d9").WithAlpha(150)),
                    GeometrySize = 8,
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#001d37"), 2),
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#2196ff")),
                    LineSmoothness = 0.5,
                    Stroke = new SolidColorPaint(SKColor.Parse("#001d37"), 3)
                }
            };

            OnPropertyChanged(nameof(StatusDistributionSeries));
            OnPropertyChanged(nameof(RoomRequestCountSeries));
            OnPropertyChanged(nameof(DateTrendSeries));
            OnPropertyChanged(nameof(StatusLabels));
            OnPropertyChanged(nameof(RoomLabels));
            OnPropertyChanged(nameof(RoomRequestXAxes));
            OnPropertyChanged(nameof(DateXAxes));
        }
        #endregion

        #region Обновление данных
        public void RefreshData()
        {
            _masterRepairs = new ObservableCollection<Repair>(_database.GetRepairs());

            RoomList = new ObservableCollection<Room>(_database.GetRooms());
            ResidentList = new ObservableCollection<Resident>(_database.GetResidents());

            CalculateRepairStatistics();

            OnPropertyChanged(nameof(RoomList));
            OnPropertyChanged(nameof(ResidentList));

            OnPropertyChanged(nameof(RoomRequestXAxes));
            OnPropertyChanged(nameof(DateXAxes));

            if (_isFiltered && ResidentId != null)
            {
                var filtered = _masterRepairs.Where(r => r.ResidentId?.Id == ResidentId.Id).ToList();
                Repairs = new ObservableCollection<Repair>(filtered);
                FilterButtonText = "Показать всю историю";
            }
            else
            {
                Repairs = new ObservableCollection<Repair>(_masterRepairs);
                _isFiltered = false;
                FilterButtonText = "Показать историю текущего пользователя";
            }

            OnPropertyChanged(nameof(Repairs));
            OnPropertyChanged(nameof(FilterButtonText));

            ApplyResidentFilter();
        }
        #endregion
    }
}
