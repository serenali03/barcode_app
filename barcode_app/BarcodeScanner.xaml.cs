namespace barcode_app;
using Camera.MAUI;
using Camera.MAUI.ZXing;

public partial class BarcodeScanner : ContentPage
{
	public BarcodeScanner()
	{
		InitializeComponent();

		cameraView.BarCodeDecoder = new ZXingBarcodeDecoder();
		cameraView.BarCodeOptions = new BarcodeDecodeOptions
		{
			AutoRotate = true,
			PossibleFormats = { BarcodeFormat.QR_CODE, BarcodeFormat.All_1D },
			ReadMultipleCodes = false,
			TryHarder = false,
			TryInverted = true
		};
		cameraView.BarCodeDetectionEnabled = true;
	}


	private void cameraView_CamerasLoaded(object sender, EventArgs e)
	{
		if (cameraView.Cameras.Count > 0)
		{
			cameraView.Camera = cameraView.Cameras.First();
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				await cameraView.StopCameraAsync();
				await cameraView.StartCameraAsync();
			});
		}
	}

	private void cameraView_BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
	{
		Shell.Current.GoToAsync(
		$"..?format={args.Result[0].BarcodeFormat}&barcode={args.Result[0].Text}");
	}
}