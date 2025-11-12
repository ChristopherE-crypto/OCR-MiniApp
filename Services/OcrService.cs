using Docnet.Core;
using Docnet.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Tesseract;

namespace OCR_MiniApp.Services
{
    public class OcrResult
    {
        public string FileName { get; set; } = "";
        public int PageCount { get; set; }
        public List<OcrPage> Pages { get; set; } = new();
    }

    public class OcrPage
    {
        public int PageNumber { get; set; }
        public string Text { get; set; } = "";
    }
    public class OcrService
    {
        private readonly string _tessDataPath;
         
        public OcrService()
        {
            _tessDataPath = @"C:\Program Files\Tesseract-OCR\tessdata";
        }

        public OcrResult ExtractText(string fileName, string filePath)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            if (extension == ".pdf")
            {
                return ExtractTextFromPdf(fileName, filePath);
            }
            else
            {
                return ExtractTextFromImage(fileName, filePath);
            }
        }

        private OcrResult ExtractTextFromImage(string fileName, string imagePath)
        {
            using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);

            return new OcrResult
            {
                FileName = fileName,
                PageCount = 1,
                Pages = new List<OcrPage>
                {
                    new OcrPage { PageNumber = 1, Text = page.GetText() }
                }
            };
        }

        private OcrResult ExtractTextFromPdf(string fileName, string pdfPath)
        {
            using var docReader = DocLib.Instance.GetDocReader(pdfPath, new PageDimensions(1080, 1920));
            var result = new OcrResult
            {
                FileName = fileName,
                PageCount = docReader.GetPageCount()
            };

            for (int i = 0; i < result.PageCount; i++)
            {
                using var pageReader = docReader.GetPageReader(i);
                var rawBytes = pageReader.GetImage();

                using var image = Image.LoadPixelData<Bgra32>(rawBytes, pageReader.GetPageWidth(), pageReader.GetPageHeight());

                image.Mutate(x => x.Grayscale().Contrast(1.2f).BinaryThreshold(0.5f)); // image pre-processing (grayscale conversion, contrast adjustement, thresholding)

                var tempImagePath = Path.GetTempFileName();
                image.Save(tempImagePath, new PngEncoder());

                var pageText = ExtractTextFromImage(fileName, tempImagePath).Pages[0].Text;
                result.Pages.Add(new OcrPage { PageNumber = i + 1, Text = pageText });

                File.Delete(tempImagePath);
            }
            return result;
        }
    }
}
