using System;
using System.Collections.Generic;
using System.Text;

namespace Dimitrova_3_bldg_3.Model
{
    public class Resident
    {
        public int Id { get; set; } // идентификатор проживающего
        public string FirstName { get; set; } // Фамилия
        public string LastName { get; set; } // Имя
        public string? MiddleName { get; set; } // Отчество
        public Room RoomId { get; set; } // Номер комнаты
        public string Faculty { get; set; } // Факультет
        public int Course { get; set; } // Курс
        public DateTime DateOfBirth { get; set; } // Дата рождения
        public string Phone { get; set; } // Номер телефона
        public string Email { get; set; } // Адресс электронной почты

        public string PickerDisplay => $"{FirstName} {LastName} {MiddleName}".Trim();

        public override string ToString()
        {
            string course = Course switch
            {
                1 => "1 курс",
                2 => "2 курс",
                3 => "3 курс",
                4 => "4 курс",
                5 => "1 курс магистратуры",
                6 => "2 курс магистратуры",
                _ => "БАГ"
            };
            string faculty = Faculty switch
            {
                "ФГМУ - Факультет государственного и муниципального управления" => "ФГМУ",
                "ФЭФ - Факультет экономики и финансов" => "ФЭФ",
                "ФБТ - Факультет безопасности и таможни" => "ФБТ",
                "ЮФ - Юридический факультет" => "ЮФ",
                "ФМОПИ - Факультет международных отношений и политических исследований" => "ФМОПИ",
                "ФСТ - Факультет социальных технологий" => "ФСТ",
                "ФСПО - Факультет среднего профессионального образования" => "ФСПО",
                _ => "БАГ",
            };

            return $" {FirstName} {LastName} {MiddleName} | {faculty} - {course} | Комната {RoomId?.Number}";
        }
        public override bool Equals(object? obj)
        {
            return this.Id == (obj as Resident)?.Id;
        }
        public override int GetHashCode()
        {
            return this.Id;
        }
    }
}
