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

        private async Task GetTextFromImage()
        {
            try
            {
                if (_module == null) { TextFromImage = "JS module not loaded."; return; }

                // Option A: capture the displayed image element into canvas first
                await _module.InvokeVoidAsync("imageElementToCanvas", "myImage", "myCanvas");

                // Get base64 PNG from canvas
                var base64 = await _module.InvokeAsync<string>("getCanvasAsBase64", "myCanvas");
                if (string.IsNullOrEmpty(base64)) { TextFromImage = "No image data."; return; }

                var imageBytes = Convert.FromBase64String(base64);

                // Tesseract usage
                using var img = Pix.LoadFromMemory(imageBytes);
                //string tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata"); // adjust path
                string pathLibrary = @"C:\\Users\\aborj\\Documents\\GitHub\\OzoraSoft-Portfolio\\Libraries";  //AppContext.BaseDirectory;
                string tessDataPath = Path.Combine(pathLibrary, "tessdata");
                using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
                using var page = engine.Process(img);
                TextFromImage = page.GetText();
            }
            catch (Exception ex)
            {
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
