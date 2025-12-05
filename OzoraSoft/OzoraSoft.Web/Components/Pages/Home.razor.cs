using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using IronOcr;

namespace OzoraSoft.Web.Components.Pages
{
    public partial class Home : ComponentBase
    {
        public string TextToShow = "";

        protected override async Task OnInitializedAsync()
        {

        }

        protected void ReadOCR()
        {
            TextToShow = "Reading...";
            var ocr = new IronTesseract();
            using var input = new OcrInput();
            input.LoadImage("C:\\Users\\aborj\\OneDrive\\Pictures\\BankEvidence-20251203.jpg");
            var result = ocr.Read(input);
            TextToShow  = result.Text;
        }
    }
}
