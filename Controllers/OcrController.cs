using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCR_MiniApp.Services;

namespace OCR_MiniApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OcrController : ControllerBase
    {
        private readonly OcrService _ocrService;

        public OcrController()
        {
            _ocrService = new OcrService();
        }

        [HttpPost("extract")]
        [Consumes("multipart/form-data")] // tells Swagger and ASP.NET that this endpoint accepts file uploads
        public async Task<IActionResult> ExtractText(IFormFile file) // IFormFile is a stream/flow of bytes -> write that stream into a temporary file which is then used by Tesseract
        {
            if (file == null || file.Length == 0) return BadRequest("No image file uploaded."); // check for missing or empty files

            var tempPath = Path.GetTempFileName();

            using (var stream = System.IO.File.Create(tempPath))
            {
                await file.CopyToAsync(stream);
            }

            var result = _ocrService.ExtractText(file.FileName, tempPath); // Tesseract expects a file path
            System.IO.File.Delete(tempPath);

            return Ok(result);
        }

    }
}
