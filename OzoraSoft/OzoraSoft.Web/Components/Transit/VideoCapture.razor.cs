using IronOcr;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Tesseract;

namespace OzoraSoft.Web.Components.Transit
{
    public partial class VideoCapture
    {
        [Parameter] public byte[]? Image { get; set; }

        [Inject] private IJSRuntime JS { get; set; } = default!;
        private IJSObjectReference? _module;
        private string TextFromImage { get; set; } = string.Empty;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/transit.js");
                await _module.InvokeVoidAsync("startVideo", "webcam");
                await _module.InvokeVoidAsync("imageElementToCanvas", "myImage", "myCanvas");
            }
        }

        private async Task StartVideo()
        {
            if (_module == null) return;
            await _module.InvokeVoidAsync("startVideo", "webcam");
        }

        private async Task StopVideo()
        {
            if (_module == null) return;
            await _module.InvokeVoidAsync("stopVideo", "webcam");
        }

        private async Task CaptureFromVideo()
        {
            if (_module == null) return;
            await _module.InvokeVoidAsync("captureToCanvas", "webcam", "myCanvas");
            StateHasChanged();
        }

        private async Task GetImage()
        {
            try
            {
                if (_module == null) { TextFromImage = "JS module not loaded."; return; }

                // get from image
                await _module.InvokeVoidAsync("imageElementToCanvas", "myImage", "myCanvas");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TextFromImage = $"Error during OCR: {ex.Message}";
            }
            StateHasChanged();
        }

        private async Task ApplyOCR()
        {
            try
            {
                if (_module == null) { TextFromImage = "JS module not loaded."; return; }

                //  capture the displayed image element into canvas first
                //await _module.InvokeVoidAsync("captureToCanvas", "webcam", "myCanvas");
                
                // get from image
                //await _module.InvokeVoidAsync("imageElementToCanvas", "myImage", "myCanvas");

                // Get base64 PNG from canvas
                var base64 = await _module.InvokeAsync<string>("getCanvasAsBase64", "myCanvas");
                if (string.IsNullOrEmpty(base64)) { TextFromImage = "No image data."; return; }

                var imageBytes = Convert.FromBase64String(base64);

                // Component: IronOCR (Trial finished):
                //IronOcr.License.LicenseKey = "IRONSUITE.CONTACT.OZORASOFT.CA.15217-2AC70754D2-HAI2BJJQNPLIRO-HDEIKQIW3ALG-6C6N67WWALS7-7B6PUAOZGMYO-SXC33P5K2PJ2-3Y6J2GKF47XF-GCYYHC-TL4WHVUPVG6QUA-DEPLOYMENT.TRIAL-CAWYQ2.TRIAL.EXPIRES.12.JAN.2026";
                //var ocr = new IronTesseract();
                //using var input = new OcrInput();
                //input.LoadImage(imageBytes);
                //var result = ocr.Read(input);
                //TextFromImage = result.Text;


                // Component: Tesseract (Free - in testing)
                using var img = Pix.LoadFromMemory(imageBytes);
                //string tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata"); // adjust path
                string pathLibrary = @"C:\\Users\\aborj\\Documents\\GitHub\\OzoraSoft-Portfolio\\Libraries";  //AppContext.BaseDirectory;
                string tessDataPath = Path.Combine(pathLibrary, "tessdata");
                using var engine = new TesseractEngine(tessDataPath, "eng+fra+hun+lat", EngineMode.Default);
                using var page = engine.Process(img);
                var confidence = $"{page.GetMeanConfidence()}"; // 0.0 – 1.0

                TextFromImage = $"[OCR confidence:{confidence}] \n{page.GetText()}";

                using var iter = page.GetIterator();
                iter.Begin();

                do
                {
                    string text = iter.GetText(PageIteratorLevel.Word);
                    float wordConf = iter.GetConfidence(PageIteratorLevel.Word);
                    Console.WriteLine($"{text} (conf: {wordConf})");
                } while (iter.Next(PageIteratorLevel.Word));

            }
            catch (JSException ex)
            {
                Console.WriteLine("Interop error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                TextFromImage = $"Error during OCR: {ex.Message}";
            }
            StateHasChanged();
        }

        private void ResetText()
        {
            TextFromImage = string.Empty;
        }

        public async ValueTask DisposeAsync()
        {
            if (_module != null) await _module.DisposeAsync();
        }
    }
}
