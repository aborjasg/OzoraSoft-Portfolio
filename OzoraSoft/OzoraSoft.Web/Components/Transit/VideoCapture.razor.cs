using Azure.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.JSInterop;
using Microsoft.VisualStudio.TextTemplating;
using OzoraSoft.DataSources.Shared;
using OzoraSoft.Library.Enums.Shared;
using System;
using Tesseract;

namespace OzoraSoft.Web.Components.Transit
{
    public partial class VideoCapture
    {
        [Parameter] public byte[]? Image { get; set; }

        [Inject] private IJSRuntime JS { get; set; } = default!;
        private IJSObjectReference? _module;
        private string _accessToken = "";
        private int captureId = 0;
        protected string TextFromImage { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            var result = await ApiUtilsClient.AccessToken_Get();
            _accessToken = result.access_token;
        }

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
                var processDateStart = DateTime.Now;

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


                // Component: Tesseract (Free - in review)
                using var img = Pix.LoadFromMemory(imageBytes);
                //string tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata"); // adjust path
                string pathLibrary = @"C:\\Users\\aborj\\Documents\\GitHub\\OzoraSoft-Portfolio\\Libraries";  //AppContext.BaseDirectory;
                string tessDataPath = Path.Combine(pathLibrary, "tessdata");
                using var engine = new TesseractEngine(tessDataPath, "eng+fra+hun+lat", EngineMode.Default);
                using var page = engine.Process(img);
                var confidence = $"OCR confidence:{page.GetMeanConfidence()}"; // 0.0 – 1.0

                TextFromImage = $"{page.GetText()}";                                

                if (!string.IsNullOrEmpty(TextFromImage.Trim()))
                {
                    using var iter = page.GetIterator();
                    iter.Begin();

                    do
                    {
                        string text = iter.GetText(PageIteratorLevel.Word);
                        float wordConf = iter.GetConfidence(PageIteratorLevel.Word);
                        Console.WriteLine($"{text} (conf: {wordConf})");
                    } while (iter.Next(PageIteratorLevel.Word));

                    // Call API EventLog
                    var processDuration = (DateTime.Now - processDateStart).TotalMilliseconds;
                    var resultId = ApiServicesClient.EventLogs_Add(new EventLog()
                    {
                        project_id = (int)enumEventLogProject.OzoraSoft_Web,
                        module_id = (int)enumEventLogModule.Transit,
                        controller_id = (int)enumEventLogController.Transit_RealTimeStream,
                        action_id = (int)enumEventLogAction.Execute,
                        action_result = $"{confidence}",
                        process_duration = processDuration,
                        process_datetime = processDateStart,
                        user_name = "Alex BG",
                        user_ip_address = "127.0.0.1"
                    }, _accessToken);
                    Console.WriteLine($"EventLog Id={resultId}]");
                }
                else
                {
                    TextFromImage = "[No text recognized]";
                }
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
            finally
            {
                StateHasChanged();
            }
        }

        private async Task SaveImage()
        {
            try
            {
                if (_module == null) { TextFromImage = "JS module not loaded."; return; }

                // Get base64 PNG from canvas
                var base64 = await _module.InvokeAsync<string>("getCanvasAsBase64", "myCanvas");
                if (string.IsNullOrEmpty(base64)) { TextFromImage = "No image data."; return; }

                var imageBytes = Convert.FromBase64String(UtilsForMessages.Compress(base64));

                var result = ApiServicesClient.VideoCaptures_Add(new OzoraSoft.DataSources.Transit.VideoCapture()
                {
                    videodevice_id = 1, // Logitech WebCam
                    image = imageBytes,
                    status = true
                }, _accessToken);
                if (result.IsCompleted)
                {
                    captureId = result.Result;
                    Console.WriteLine($"New record Id={captureId}]");                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                StateHasChanged();
            }
        }

        private async Task LoadImageFromDB()
        {
            try
            {
                if (_module == null) { TextFromImage = "JS module not loaded."; return; }
                captureId = 8;
                if (captureId > 0)
                {
                    var record = await ApiServicesClient.VideoCaptures_Get(captureId, _accessToken);
                    if (record != null)
                    {                        
                        var imageBytes = UtilsForMessages.Decompress(Convert.ToBase64String(record!.image!));
                        await _module.InvokeAsync<string>("setCanvasAsBase64", "myCanvas", imageBytes);
                        Console.WriteLine($"New record Id={captureId}]");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                StateHasChanged();
            }
        }

        private void ResetText()
        {
            TextFromImage = string.Empty;
        }

        public async ValueTask DisposeAsync()
        {
            if (_module != null)
            {
                await StopVideo();
                await _module.DisposeAsync();
            }
        }
    }
}
