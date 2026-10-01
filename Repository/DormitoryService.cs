using Dimitrova_3_bldg_3.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Dimitrova_3_bldg_3.Repository
{
    public class DormitoryService : DbContext
    {
        #region Создание БД
        DbSet<Room> Rooms => Set<Room>();
        DbSet<Resident> Residents => Set<Resident>();
        DbSet<Repair> Repairs => Set<Repair>();

        public DormitoryService(DbContextOptions options) : base(options)
        {
            // Database.EnsureDeleted(); // Удаление БД
            Database.EnsureCreated(); // Создание БД
        }
        #endregion

        #region Операции с комнатами
        public ObservableCollection<Room> GetRooms()
        {
            return new ObservableCollection<Room>(Rooms.OrderBy(r => r.Section).ThenBy(r => r.Number).ToList());
        }
        public async Task AddRoom(Room room)
        {
            Rooms.Add(room);
            await SaveChangesAsync();
        }
        public async Task UpdateRoom(Room room)
        {
            Rooms.Update(room);
            await SaveChangesAsync();
        }
        public async Task RemoveRoom(Room room)
        {
            Rooms.Remove(room);
            await SaveChangesAsync();
        }
        #endregion

        #region Операции с проживающими
        public ObservableCollection<Resident> GetResidents()
        {
            return new ObservableCollection<Resident>(Residents.OrderBy(r => r.FirstName).ThenBy(r => r.LastName).ThenBy(r => r.MiddleName).ToList());
        }
        public async Task AddResident(Resident resident)
        {
            Residents.Add(resident);
            await SaveChangesAsync();
        }
        public async Task UpdateResident(Resident resident)
        {
            Residents.Update(resident);
            await SaveChangesAsync();
        }
        public async Task RemoveResident(Resident resident)
        {
            Residents.Remove(resident);
            await SaveChangesAsync();
        }
        #endregion

        #region Операции с заявками
        public ObservableCollection<Repair> GetRepairs()
        {
            return new ObservableCollection<Repair>(Repairs.OrderByDescending(r => r.RequestDate).ToList());
        }
        public async Task AddRepair(Repair repair)
        {
            Repairs.Add(repair);
            await SaveChangesAsync();
        }
        public async Task UpdateRepair(Repair repair)
        {
            Repairs.Update(repair);
            await SaveChangesAsync();
        }
        public async Task RemoveRepair(Repair repair)
        {
            Repairs.Remove(repair);
            await SaveChangesAsync();
        }
        #endregion

        #region Каскадное удаление
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Комната → Проживающие
            modelBuilder.Entity<Resident>()
                .HasOne(r => r.RoomId)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            // Комната → Заявки
            modelBuilder.Entity<Repair>()
                .HasOne(r => r.RoomId)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            // Проживающий → Заявки
            modelBuilder.Entity<Repair>()
                .HasOne(r => r.ResidentId)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
        }
        #endregion
    }
}
