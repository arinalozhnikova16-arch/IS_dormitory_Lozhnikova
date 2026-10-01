using System;
using System.Collections.Generic;
using System.Text;

namespace Dimitrova_3_bldg_3.Model
{
    public class Room
    {
        public int Id { get; set; } // Идентификатор комнаты
        public int Number {  get; set; } // Номер комнаты
        public int Section { get; set; } // Номер секции
        public string Gender { get; set; } // Тип комнаты
        public double Floor { get; set; } // Номер этажа
        public double Area { get; set; } // Площадь
        public int TotalBeds { get; set; } // Общее кол-во койко-мест
        public int OccupiedBeds { get; set; } // Кол-во занятых койко-мест

        public string PickerDisplay => $"Комната {Number} (секция {Section})";

        public override string ToString()
        {
            string gender = Gender switch
            {
                "Мужская" => "М",
                "Женская" => "Ж",
                _ => "БАГ",
            };
            return $"{Number} команата | {Section} секция | {gender} | {OccupiedBeds}/{TotalBeds} мест";
        }
        public override bool Equals(object? obj)
        {
            return this.Id == (obj as Room)?.Id;
        }
        public override int GetHashCode()
        {
            return this.Id;
        }
    }
}
