using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace Itemscan
{
	public partial class AzureTable : ContentPage
	{
		//MobileServiceClient client = AzureManager.AzureManagerInstance.AzureClient;
		Geocoder geoCoder;

		public AzureTable()
		{
			InitializeComponent();
			geoCoder = new Geocoder();
		}
		async void Handle_ClickedAsync(object sender, System.EventArgs e)
		{
			loading.IsRunning = true;
			List<ScanItem> ScanItemInformation = await AzureManager.AzureManagerInstance.GetScanItemInformation();

            foreach (ScanItem model in ScanItemInformation)
			{
			  var position = new Position(model.Latitude, model.Longitude);
			  var possibleAddresses = await geoCoder.GetAddressesForPositionAsync(position);
			  foreach (var address in possibleAddresses)
			      model.City = address;
			}

			ScanItemList.ItemsSource = ScanItemInformation;
			//await postLocationAsync();
			loading.IsRunning = false;
		}
	}
}
