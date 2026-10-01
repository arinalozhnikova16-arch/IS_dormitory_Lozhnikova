namespace Dimitrova_3_bldg_3
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLocationClicked(object sender, EventArgs e)
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Нужно разрешение",
                                  "Для определения вашего местоположения нужно дать разрешение.",
                                  "OK");
                return;
            }

            try
            {
                var location = await Geolocation.Default.GetLocationAsync();

                if (location != null)
                {
                    double dormLat = 59.8453;
                    double dormLon = 30.3771;

                    double distance = CalculateDistance(location.Latitude, location.Longitude, dormLat, dormLon);

                    bool openMap = await DisplayAlert(
                        "Ваше местоположение",
                        $"Вы находитесь в {distance:F1} км от общежития №2 СЗИУ РАНХиГС.\n\nПостроить маршрут?",
                        "Да", "Нет");

                    if (openMap)
                    {
                        await Map.Default.OpenAsync(dormLat, dormLon, new MapLaunchOptions
                        {
                            Name = "Общежитие №2 СЗИУ РАНХиГС (Димитрова, 3к3)",
                            NavigationMode = NavigationMode.Driving
                        });
                    }
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось определить ваше местоположение. Попробуйте включить GPS.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не получилось определить геолокацию: {ex.Message}", "OK");
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371;
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees) => degrees * Math.PI / 180;

        private async void SiteBtnClicked(object sender, EventArgs e)
        {
            await Browser.Default.OpenAsync("https://priem.spb.ranepa.ru/inogorodnim-postupayushhim/");
        }
        private async void SiteUSOBtnClicked(object sender, EventArgs e)
        {
            await Browser.Default.OpenAsync("https://uso-sziu.orgs.biz/");
        }

        private async void VkBtnClicked(object sender, EventArgs e)
        {
            await Browser.Default.OpenAsync("https://vk.com/obshchezhitie_spb");
        }

        private async void TgBtnClicked(object sender, EventArgs e)
        {
            await Browser.Default.OpenAsync("https://t.me/spbranepahouse");
        }

        private async void OnPhoneCallTapped(object sender, TappedEventArgs e)
        {
            var label = (Label)sender;
            var phoneNumber = label.Text;
            var cleanNumber = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
            PhoneDialer.Default.Open(cleanNumber);
        }

        private async void OnEmailTapped(object sender, TappedEventArgs e)
        {
            var label = sender as Label;
            string textToCopy = label.Text;
            await Clipboard.Default.SetTextAsync(textToCopy);
        }
    }
}