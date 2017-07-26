using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms;
using System.Threading.Tasks;
using Plugin.Geolocator;

namespace Itemscan
{
    public partial class ItemscanPage : ContentPage
    {
		//MobileServiceClient client = AzureManager.AzureManagerInstance.AzureClient;
         
		public ItemscanPage()
        {
            InitializeComponent();
        }
		async void Handle_ClickedAsync(object sender, System.EventArgs e)
		{
            List<ScanItem> ScanItemInformation = await AzureManager.AzureManagerInstance.GetScanItemInformation();

            ScanItemList.ItemsSource = ScanItemInformation;

            await postLocationAsync();
		}

		async Task postLocationAsync()
		{

			var locator = CrossGeolocator.Current;
			locator.DesiredAccuracy = 50;

            var position = await locator.GetPositionAsync(10000);

            ScanItem model = new ScanItem()
			{
				Longitude = (float)position.Longitude,
				Latitude = (float)position.Latitude

			};

            await AzureManager.AzureManagerInstance.PostScanItemInformation(model);
		}
    }
}
