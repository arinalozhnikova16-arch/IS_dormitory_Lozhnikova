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
    public class ResidentViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #region Поля
        private readonly DormitoryService _database;
        private int id;
        private string firstName;
        private string lastName;
        private string? middleName;
        private Room roomId;
        private string faculty;
        private int course;
        private DateTime _dateOfBirth;
        private string phone;
        private string email;
        #endregion

        #region Свойства фильтрации, сортировки и поиска
        public string FacultyFilter { get; set; } = "Все";
        public string CourseFilter { get; set; } = "Все";
        public string RoomFilter { get; set; }
        public string ResidentSortOption { get; set; } = "По фамилии";
        public string SearchQuery { get; set; }
        #endregion

        #region Диаграммы (Series и подписи)
        public ISeries[] FacultyDistributionSeries { get; set; } = Array.Empty<ISeries>();
        public ISeries[] CourseDistributionSeries { get; set; } = Array.Empty<ISeries>();

        public string[] FacultyLabels { get; set; } = Array.Empty<string>();
        public string[] CourseLabels { get; set; } = Array.Empty<string>();
        public Axis[] CourseXAxes { get; set; } = Array.Empty<Axis>();
        public Axis[] FacultyXAxes { get; set; } = Array.Empty<Axis>();
        #endregion

        #region Коллекция и свойства проживающего
        public ObservableCollection<Resident> Residents { get; set; } = [];

        private ObservableCollection<Room> _roomList = new ObservableCollection<Room>();

        public ObservableCollection<Room> RoomList
        {
            get => _roomList;
            set
            {
                _roomList = value;
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
        } // идентификатор проживающего
        public string FirstName
        {
            get => firstName;
            set
            {
                if (firstName == value) return;
                firstName = value;
                OnPropertyChanged();
            }
        } // Фамилия
        public string LastName
        {
            get => lastName;
            set
            {
                if (lastName == value) return;
                lastName = value;
                OnPropertyChanged();
            }
        } // Имя
        public string? MiddleName
        {
            get => middleName;
            set
            {
                if (middleName == value) return;
                middleName = value;
                OnPropertyChanged();
            }
        } // Отчество
        public string RoomDisplay => RoomId?.PickerDisplay ?? "Комната не выбрана";
        public Room RoomId
        {
            get => roomId;
            set
            {
                if (roomId == value) return;
                roomId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RoomDisplay));
            }
        } // Номер комнаты
        public string Faculty
        {
            get => faculty;
            set
            {
                if (faculty == value) return;
                faculty = value;
                OnPropertyChanged();
            }
        } // Факультет
        public int Course
        {
            get => course;
            set
            {
                if (course == value) return;
                course = value;
                OnPropertyChanged();
            }
        } // Курс
        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (_dateOfBirth == value) return;
                _dateOfBirth = value;
                OnPropertyChanged();
            }
        }
        public string Phone
        {
            get => phone;
            set
            {
                if (phone == value) return;
                phone = value;
                OnPropertyChanged();
            }
        } // Номер телефона
        public string Email
        {
            get => email;
            set
            {
                if (email == value) return;
                email = value;
                OnPropertyChanged();
            }
        } // Адресс электронной почты
        #endregion

        #region ICommands
        public ICommand AddCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand UpdateCommand { get; set; }
        public ICommand SelectedItemCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand FilterCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand SortResidentsCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        #endregion

        public ResidentViewModel(DormitoryService database)
        {
            _database = database;
            this.Residents = new ObservableCollection<Resident>(_database.GetResidents());

            this.RoomList = new ObservableCollection<Room>(_database.GetRooms());

            this.DateOfBirth = DateTime.Today;

            #region Инициализация CRUD

            // Добавление
            AddCommand = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(FirstName)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите фамилию", "ОК"); return; }
                if (string.IsNullOrWhiteSpace(LastName)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите имя", "ОК"); return; }
                if (RoomId == null) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите комнату", "ОК"); return; }
                if (string.IsNullOrWhiteSpace(Faculty)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите факультет", "ОК"); return; }
                if (Course < 1 || Course > 6) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Курс должен быть от 1 до 6", "ОК"); return; }
                if (string.IsNullOrWhiteSpace(Phone)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите номер телефона", "ОК"); return; }
                if (string.IsNullOrWhiteSpace(Email)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите адрес электронной почты", "ОК"); return; }
                if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains("@"))
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Email должен содержать символ '@'", "ОК"); return;
                }

                Resident resident = new()
                {
                    FirstName = this.FirstName,
                    LastName = this.LastName,
                    MiddleName = this.MiddleName,
                    RoomId = this.RoomId,
                    Faculty = this.Faculty,
                    Course = this.Course,
                    DateOfBirth = this.DateOfBirth,
                    Phone = this.Phone,
                    Email = this.Email,
                };
                this.RoomId.OccupiedBeds++;
                _database.UpdateRoom(this.RoomId);
                Residents.Add(resident);
                _database.AddResident(resident);
                Vibration.Default.Vibrate(200);
                await Application.Current.MainPage.DisplayAlert("Успех", "Проживающий добавлен", "ОК");
                RefreshData();
                OnPropertyChanged(nameof(Residents));
            });

            // Удаление
            RemoveCommand = new Command<Resident>(async (resident) =>
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert("Удаление", "Удалить проживающего? Все его заявки будут удалены.", "Да", "Нет");
                if (!confirm) return;

                this.RoomId.OccupiedBeds--;
                _database.UpdateRoom(this.RoomId);
                Residents.Remove(resident);
                _database.RemoveResident(resident);
                Vibration.Default.Vibrate(200);
                await Application.Current.MainPage.DisplayAlert("Успех", "Проживающий удалён", "ОК");
                RefreshData();
                OnPropertyChanged(nameof(Residents));
            }, (resident) => { return resident != null; });
  
            // Обновление
            UpdateCommand = new Command<Resident>(async (resident) =>
            {
                if (string.IsNullOrWhiteSpace(FirstName)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите фамилию", "ОК"); return; }
                if (string.IsNullOrWhiteSpace(LastName)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите имя", "ОК"); return; }
                if (RoomId == null) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите комнату", "ОК"); return; }
                if (string.IsNullOrWhiteSpace(Faculty)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите факультет", "ОК"); return; }
                if (Course < 1 || Course > 6) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Курс должен быть от 1 до 6", "ОК"); return; }
                if (string.IsNullOrWhiteSpace(Phone)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите номер телефона", "ОК"); return; }
                if (string.IsNullOrWhiteSpace(Email)) { await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите адрес электронной почты", "ОК"); return; }
                if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains("@"))
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Email должен содержать символ '@'", "ОК"); return;
                }

                var oldRoom = resident.RoomId;
                var newRoom = this.RoomId;

                int i = Residents.IndexOf(resident);
                resident.FirstName = this.FirstName;
                resident.LastName = this.LastName;
                resident.MiddleName = this.MiddleName;
                resident.RoomId = newRoom;
                resident.Faculty = this.Faculty;
                resident.Course = this.Course;
                resident.DateOfBirth = this.DateOfBirth;
                resident.Phone = this.Phone;
                resident.Email = this.Email;

                if (oldRoom != null && newRoom != null && oldRoom.Id != newRoom.Id)
                {
                    oldRoom.OccupiedBeds--;
                    _database.UpdateRoom(oldRoom);

                    newRoom.OccupiedBeds++;
                    _database.UpdateRoom(newRoom);
                }

                Residents[i] = resident;
                _database.UpdateResident(resident);
                Vibration.Default.Vibrate(200);
                await Application.Current.MainPage.DisplayAlert("Успех", "Данные обновлены", "ОК");
                RefreshData();
                OnPropertyChanged(nameof(Residents));
            }, (resident) => { return resident != null; });
      
            // Выбранный элемент
            SelectedItemCommand = new Command<Resident>((resident) =>
            {
                FirstName = resident.FirstName;
                LastName = resident.LastName;
                MiddleName = resident.MiddleName;
                RoomId = resident.RoomId;
                Faculty = resident.Faculty;
                Course = resident.Course;
                DateOfBirth = resident.DateOfBirth;
                Phone = resident.Phone;
                Email = resident.Email; 
            }, (person) => { return person != null; });
            #endregion

            #region Очистка полей, фильтрация, сортировки и поиск

            // Очистка полей формы
            ClearCommand = new Command(() =>
            {
                Id = 0;
                FirstName = string.Empty;
                LastName = string.Empty;
                MiddleName = string.Empty;
                RoomId = null;
                Faculty = string.Empty;
                Course = 0;
                DateOfBirth = DateTime.Today;
                Phone = string.Empty;
                Email = string.Empty;
            });

            // Фильтрация
            FilterCommand = new Command(() => {
                var q = _database.GetResidents().AsEnumerable();
                if (FacultyFilter != "Все") q = q.Where(r => r.Faculty == FacultyFilter);
                if (CourseFilter != "Все" && int.TryParse(CourseFilter, out int c)) q = q.Where(r => r.Course == c);
                if (int.TryParse(RoomFilter, out int rn)) q = q.Where(r => r.RoomId?.Number == rn);
                Residents = new ObservableCollection<Resident>(q);
                OnPropertyChanged(nameof(Residents));
            });

            // Очистка фильтрации и поиска
            ResetCommand = new Command(() => {
                FacultyFilter = "Все"; CourseFilter = "Все"; RoomFilter = string.Empty;
                Residents = new ObservableCollection<Resident>(_database.GetResidents());
                OnPropertyChanged(nameof(FacultyFilter));
                OnPropertyChanged(nameof(CourseFilter));
                OnPropertyChanged(nameof(RoomFilter));
                OnPropertyChanged(nameof(Residents));
            });

            // Сортировка
            SortResidentsCommand = new Command(() => {
                var q = Residents.AsEnumerable();
                q = ResidentSortOption switch
                {
                    "По фамилии" => q.OrderBy(r => r.FirstName),
                    "По имени" => q.OrderBy(r => r.LastName),
                    "По отчеству" => q.OrderBy(r => r.MiddleName ?? ""),
                    "По комнате" => q.OrderBy(r => r.RoomId?.Number ?? 0),
                    _ => q
                };
                Residents = new ObservableCollection<Resident>(q);
                OnPropertyChanged(nameof(Residents));
            });

            // Поиск по фамилии
            SearchCommand = new Command(() => {
                var q = Residents.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                    q = q.Where(r => r.FirstName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
                Residents = new ObservableCollection<Resident>(q);
                OnPropertyChanged(nameof(Residents));
            });
            #endregion
        }

        #region Диаграммы
        private void CalculateResidentStatistics()
        {
            var allResidents = _database.GetResidents().ToList();

            // 1. Распределение по факультетам
            var facultyGroups = allResidents.GroupBy(r => r.Faculty)
                                .OrderBy(g => g.Key)
                                .ToList();

            var facultyValues = new List<int>();
            var facultyNames = new List<string>();

            foreach (var group in facultyGroups)
            {
                facultyValues.Add(group.Count());
                string shortName = group.Key.Contains(" - ") ? group.Key.Split(" - ")[0] : group.Key;
                facultyNames.Add(shortName);
            }

            FacultyXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = facultyNames.ToArray(),
                    LabelsRotation = -45,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray),
                    TicksPaint = new SolidColorPaint(SKColors.Black)
                }
            };

            FacultyDistributionSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = facultyValues.ToArray(),
                    Name = "Студенты",
                    Fill = new SolidColorPaint(SKColor.Parse("#0058a7"))
                }
            };

            // 2. Распределение по курсам
            var courseGroups = allResidents.GroupBy(r => r.Course)
                                           .OrderBy(g => g.Key)
                                           .ToList();

            var courseValues = new List<int>();
            var courseNames = new List<string>();

            foreach (var group in courseGroups)
            {
                courseValues.Add(group.Count());
                courseNames.Add($"{group.Key} курс");
            }

            CourseXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = courseNames.ToArray(),
                    LabelsRotation = 0,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray),
                    TicksPaint = new SolidColorPaint(SKColors.Black)
                }
            };

            CourseDistributionSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Values = courseValues.ToArray(),
                    Name = "Студенты",
                    Fill = new SolidColorPaint(SKColor.Parse("#001d37"))
                }
            };

            CourseLabels = courseNames.ToArray();

            OnPropertyChanged(nameof(FacultyDistributionSeries));
            OnPropertyChanged(nameof(CourseDistributionSeries));
            OnPropertyChanged(nameof(FacultyLabels));
            OnPropertyChanged(nameof(CourseLabels));
            OnPropertyChanged(nameof(CourseXAxes));
            OnPropertyChanged(nameof(FacultyXAxes));
        }
        #endregion

        #region Обновление данных
        public void RefreshData()
        {
            Residents = new ObservableCollection<Resident>(_database.GetResidents());
            RoomList = new ObservableCollection<Room>(_database.GetRooms());

            CalculateResidentStatistics();

            OnPropertyChanged(nameof(Residents));
            OnPropertyChanged(nameof(RoomList));
            OnPropertyChanged(nameof(CourseXAxes));
            OnPropertyChanged(nameof(FacultyXAxes));
        }
        #endregion
    }
}
