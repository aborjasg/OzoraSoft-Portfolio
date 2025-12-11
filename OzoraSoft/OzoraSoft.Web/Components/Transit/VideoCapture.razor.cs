using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Tesseract;

namespace OzoraSoft.Web.Components.Transit
{
    public partial class VideoCapture
    {
        //[Inject] private IJSRuntime JS { get; set; } = default!;
        private string TextToShow { get; set; } = string.Empty;
        private byte[]? imageBytes;

        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{
        //    if (firstRender)
        //    {
        //        //imageBytes = await JS.InvokeAsync<byte[]>("getImageBytes", "myImage");
        //    }
        //}

        protected void RunOCRcomponent()
        {
            try
            {
                //imageBytes = await JS!.InvokeAsync<byte[]>("getImageBytes", "myImage");
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    TextToShow = "Reading...";
                    //var ocr = new IronTesseract();
                    //using var input = new OcrInput();
                    //input.LoadImage(imageBytes);
                    //var result = ocr.Read(input);
                    //TextToShow = result.Text;
                    using var img = Pix.LoadFromMemory(imageBytes);
                    string pathLibrary = @"C:\\Users\\aborj\\Documents\\GitHub\\OzoraSoft-Portfolio\\Libraries";  //AppContext.BaseDirectory;
                    string tessDataPath = Path.Combine(pathLibrary, "tessdata");
                    using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
                    using var page = engine.Process(img);
                    TextToShow = page.GetText();
                }
                else
                {
                    TextToShow = "No image data available.";
                }
            }
            catch (Exception ex)
            {
                TextToShow = $"Error during OCR processing: {ex.Message}";
            }
        }

        private void ResetResults()
        {
            TextToShow = string.Empty;
        }
    }
}
