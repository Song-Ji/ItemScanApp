using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using System.Linq;
using Plugin.Geolocator;
using Newtonsoft.Json;

namespace Itemscan
{
	public partial class CustomVision : ContentPage
	{
		public CustomVision()
		{
			InitializeComponent();
		}

		private async void loadCamera(object sender, EventArgs e)
		{
			await CrossMedia.Current.Initialize();

			if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
			{
				await DisplayAlert("No Camera", ":( No camera available.", "OK");
				return;
			}

			MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
			{
				PhotoSize = PhotoSize.Medium,
				Directory = "Sample",
				Name = $"{DateTime.UtcNow}.jpg"
			});

			if (file == null)
				return;

			image.Source = ImageSource.FromStream(() =>
			{
				return file.GetStream();
			});

			await postLocationAsync();

			await MakePredictionRequest(file);
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

		static byte[] GetImageAsByteArray(MediaFile file)
		{
			var stream = file.GetStream();
			BinaryReader binaryReader = new BinaryReader(stream);
			return binaryReader.ReadBytes((int)stream.Length);
		}

		async Task MakePredictionRequest(MediaFile file)
		{
			var client = new HttpClient();

			client.DefaultRequestHeaders.Add("Prediction-Key", "ac159fedcda1444ea37d651c15cd2080");

			string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/4804e607-10a5-4309-aef8-b14f58ecc0af/image?iterationId=af0a2089-c39f-4b72-8a5f-b7ce1edc14a3";

			HttpResponseMessage response;

			byte[] byteData = GetImageAsByteArray(file);

			using (var content = new ByteArrayContent(byteData))
			{

				content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				response = await client.PostAsync(url, content);


				if (response.IsSuccessStatusCode)
				{
                    var responseString = await response.Content.ReadAsStringAsync();
					EvaluationModel responseModel = JsonConvert.DeserializeObject<EvaluationModel>(responseString);
					double max = responseModel.Predictions.Max(m => m.Probability);
                    WhetherCar.Text = "Whether Car: ";
                    JudgeLabel.Text = (max >= 0.5) ? "Car " : "Not Car ";

					JObject rss = JObject.Parse(responseString);
					//Querying with LINQ
					//Get all Prediction Values
					var Probability = from p in rss["Predictions"] select (string)p["Probability"];
					var Tag = from p in rss["Predictions"] select (string)p["Tag"];

					//Truncate values to labels in XAML
					foreach (var item in Tag)
					{
						TagLabel.Text += item + ": \n";
					}

					foreach (var item in Probability)
					{
						PredictionLabel.Text += item + "\n";
					}
			
				}

				//Get rid of file once we have finished using it
				file.Dispose();
			}
		}
	}
}