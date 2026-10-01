using System;
using System.Collections.Generic;
using System.Text;

namespace Dimitrova_3_bldg_3.Model
{
    public class Repair
    {
        public int Id { get; set; } // Идентификатор заявки
        public string Description { get; set; } // Описание неисправности
        public Room RoomId { get; set; } // ИД команты
        public Resident ResidentId { get; set; } // ИД проживаюшего
        public DateTime RequestDate { get; set; } // Дата подачи заявления
        public string Status { get; set; } // Статус заявки
        public override string ToString()
        {
            string descriptionShort = Description.Length > 30
                ? Description.Substring(0, 27) + "..."
                : Description;

            return $"{Status} | Дата подачи: {RequestDate:dd.MM.yyyy} | {descriptionShort} | {RoomId?.Number} комната";
        }
        public override bool Equals(object? obj)
        {
            return this.Id == (obj as Repair)?.Id;
        }
        public override int GetHashCode()
        {
            return this.Id;
        }
    }
}
