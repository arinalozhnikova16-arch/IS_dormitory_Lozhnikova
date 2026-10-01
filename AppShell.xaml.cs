namespace Dimitrova_3_bldg_3
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("roomPageRoute", typeof(RoomPage));
            Routing.RegisterRoute("residentPageRoute", typeof(ResidentPage));
            Routing.RegisterRoute("repairPageRoute", typeof(RepairPage));

            Routing.RegisterRoute("roomStatsRoute", typeof(RoomStatsPage));
            Routing.RegisterRoute("residentStatsRoute", typeof(ResidentStatsPage));
            Routing.RegisterRoute("repairStatsRoute", typeof(RepairStatsPage));
        }
    }
}
