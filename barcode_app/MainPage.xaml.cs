using System.Diagnostics;
using System.Text.Json;
using System.Web;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace barcode_app{
	public partial class MainPage : ContentPage, IQueryAttributable
	{	
		HttpClient client = new HttpClient();

		public void ApplyQueryAttributes(IDictionary<string, object> query)
		{
			ArtworkEntry.Text = HttpUtility.UrlDecode(query["id"].ToString());
			Console.WriteLine(HttpUtility.UrlDecode(query["format"].ToString()));
		}

		private void Network_test()
		{
			Debug.WriteLine("Network test started");
			try
			{
				var response = client.GetAsync("http://10.0.2.2:8000/api/").Result;
				
				if (response.IsSuccessStatusCode)
				{
					Debug.WriteLine("Network test ok. " + response.StatusCode);
					LabelHttpResponse.Text = "Network ready (API reachable)";
				}
				else
				{
					Debug.WriteLine("Network test error. Status code: " + response.StatusCode);
					LabelHttpResponse.Text = "API reachable but returned error: " + response.StatusCode;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Network test fail. " + ex.Message);
				LabelHttpResponse.Text = "Network may not ready";
			}
		}

		
		

		public MainPage()
		{
			InitializeComponent();
			Routing.RegisterRoute("barcodescanner", typeof(BarcodeScanner));
			Network_test();
		}

		private async Task Show_Toast(string message)
		{
			// Create a toast message with a cancellation token
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			ToastDuration duration = ToastDuration.Short;
			double fontSize = 14;
			var toast = Toast.Make(message, duration, fontSize);
			await toast.Show(cancellationTokenSource.Token);
		}

		private void ResetProductDetail()
		{
			// Reset the item detail labels to default values
			LabelProduct.Text = "Name: ";
			LabelBrand.Text = "Artist: ";
			LabelIngredients.Text = "Description: ";
			LabelCategories.Text = "Dimensions: ";
			LabelMessage.Text = string.Empty;
			LabelMessage.TextColor = Colors.Black;
			ImageCover.Source =
			ImageSource.FromFile("image_coming_soon.png");
		}

		private void ParseInventoryJSON(string json)
		{
			try
			{
				// reset labels
				LabelProduct.Text = "Name: ";
				LabelBrand.Text = "Artist: ";
				LabelIngredients.Text = "Description: ";
				LabelCategories.Text = "Dimensions: ";
				using (var jsonDocument = JsonDocument.Parse(json))
				{
					var root = jsonDocument.RootElement;

					LabelProduct.Text = "Name: " + root.GetProperty("name").GetString();
					LabelBrand.Text = "Artist: " + root.GetProperty("artist").GetString();
					LabelIngredients.Text = "Description: " + root.GetProperty("description").GetString();
					LabelCategories.Text = "Dimensions: " + root.GetProperty("dimension").GetString();

					if (root.TryGetProperty("image", out var imageUrl))
					{
						ImageCover.Source = ImageSource.FromUri(new Uri(imageUrl.GetString()));
					}
					else
					{
						ImageCover.Source = ImageSource.FromFile("image_coming_soon.png");
					}

					LabelMessage.Text = "Artwork information loaded";
					LabelMessage.TextColor = Colors.Green;
				}
			}
			catch (Exception ex)
			{
				LabelMessage.Text = $"Error: {ex.Message}";
				LabelMessage.TextColor = Colors.Red;
				ImageCover.Source = ImageSource.FromFile("image_coming_soon.png");
			}
		}

				
		private async void FindBtn_Clicked(object sender, EventArgs e)
		{
			if (ArtworkEntry.Text.Trim().Length == 0)
			{
				// No barcode number is entered
				await Show_Toast("Please enter a barcode number");
				return;
			}

			ResetProductDetail();
			
			try
			{
				await Show_Toast("Querying product information");
				
				string ApiUrl = $"http://10.0.2.2:8000/api/{ArtworkEntry.Text.Trim()}.json";
				
				var resp = await client.GetStringAsync(ApiUrl);
				LabelHttpResponse.Text = resp;

				
				// Check if response contains valid product data
				if (resp.Contains("\"status\":0") || resp.Length < 50)
				{
					// Product not found
					LabelMessage.Text = "Product not found in database";
					LabelMessage.TextColor = Colors.Red;
					ImageCover.Source = ImageSource.FromFile("image_coming_soon.png");
					return;
				}
				
				ParseInventoryJSON(resp);

			}
			catch (Exception ex)
			{
				LabelHttpResponse.Text = "Querying product information error. " + ex.Message;
				LabelMessage.Text = "Error fetching product data " + ex.Message + ArtworkEntry.Text.Trim();
				LabelMessage.TextColor = Colors.Red;
				Debug.WriteLine(LabelHttpResponse.Text);
			}
		}


	}
}



