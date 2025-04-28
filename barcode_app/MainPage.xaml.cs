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
			BarcodeEntry.Text = HttpUtility.UrlDecode(query["barcode"].ToString());
			Console.WriteLine(HttpUtility.UrlDecode(query["format"].ToString()));
		}

		private void Network_test()
		{
			Debug.WriteLine("Network test started");
			// try to get something from internet
			try
			{
				var resp = client.GetAsync("https://world.openfoodfacts.org/api/v0/product/<barcode>.json").Result;
				Debug.WriteLine("Network test ok. " + resp.Content.ReadAsStringAsync().Result);
				LabelHttpResponse.Text = "Network ready";
			}
			// if the network is not ready, show a message
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
			LabelProduct.Text = "Product: ";
			LabelBrand.Text = "Brand: ";
			LabelIngredients.Text = "Ingredients: ";
			LabelCategories.Text = "Category: ";
			LabelMessage.Text = string.Empty;
			LabelMessage.TextColor = Colors.Black;
			ImageCover.Source =
			ImageSource.FromFile("image_coming_soon.png");
		}

		private void ParseFoodProductJSON(string json)
		{
			try
			{
				// Reset labels
				LabelProduct.Text = "Product: ";
				LabelBrand.Text = "Brand: ";
				LabelIngredients.Text = "Ingredients: ";
				LabelCategories.Text = "Category: ";
				
				// Convert http response content to JSON object
				using (var jsonDocument = JsonDocument.Parse(json))
				{
					var rootElement = jsonDocument.RootElement;
					
					// Check if product exists
					if (rootElement.TryGetProperty("status", out var status) && status.GetInt32() == 0)
					{
						LabelMessage.Text = "Product not found in database";
						LabelMessage.TextColor = Colors.Red;
						ImageCover.Source = ImageSource.FromFile("image_coming_soon.png");
						return;
					}

					if (rootElement.TryGetProperty("product", out var product))
					{
						// Get product name
						if (product.TryGetProperty("product_name", out var productName))
						{
							LabelProduct.Text += productName.ToString();
						}

						// Get brand
						if (product.TryGetProperty("brands", out var brands))
						{
							LabelBrand.Text += brands.ToString();
						}

						// Get ingredients
						if (product.TryGetProperty("ingredients_text", out var ingredients))
						{
							LabelIngredients.Text += ingredients.ToString();
						}

						// Get categories
						if (product.TryGetProperty("categories", out var categories))
						{
							LabelCategories.Text += categories.ToString();
						}

						// Get product image
						if (product.TryGetProperty("image_url", out var imageUrl))
						{
							ImageCover.Source = ImageSource.FromUri(new Uri(imageUrl.ToString()));
						}
						else
						{
							ImageCover.Source = ImageSource.FromFile("image_coming_soon.png");
						}

						LabelMessage.Text = "Product information loaded";
						LabelMessage.TextColor = Colors.Green;
					}
				}
			}
			catch (Exception ex)
			{
				LabelMessage.Text = $"Error: {ex.Message}";
				LabelMessage.TextColor = Colors.Red;
				ImageCover.Source = ImageSource.FromFile("image_coming_soon.png");
			}
		}

		private async void ScanBarcodeBtn_Clicked(object sender, EventArgs e)
		{
			await Shell.Current.GoToAsync("barcodescanner");
		}
		private async void FindBtn_Clicked(object sender, EventArgs e)
		{
			if (BarcodeEntry.Text.Trim().Length == 0)
			{
				// No barcode number is entered
				await Show_Toast("Please enter a barcode number");
				return;
			}

			ResetProductDetail();
			
			try
			{
				await Show_Toast("Querying product information");
				
				// API endpoint format: https://world.openfoodfacts.org/api/v0/product/<barcode>.json
				string ApiUrl = $"https://world.openfoodfacts.org/api/v0/product/{BarcodeEntry.Text.Trim()}.json";
				
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
				
				ParseFoodProductJSON(resp);
			}
			catch (Exception ex)
			{
				LabelHttpResponse.Text = "Querying product information error. " + ex.Message;
				LabelMessage.Text = "Error fetching product data";
				LabelMessage.TextColor = Colors.Red;
				Debug.WriteLine(LabelHttpResponse.Text);
			}
		}


	}
}



