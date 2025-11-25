using Docnet.Core;
using Docnet.Core.Models;
using SkiaSharp;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;

namespace TesseractOcrTest.Services;

public class OcrService
{
    private readonly string _tessDataPath;

    public OcrService()
    {
        _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tessdata");
    }

    public async Task<OcrResult> ProcessPdfAsync(Stream pdfStream)
    {
        var result = new OcrResult();
        var tempPdfPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

        try
        {
            using (var fileStream = File.Create(tempPdfPath))
            {
                await pdfStream.CopyToAsync(fileStream);
            }

            using (var docReader = DocLib.Instance.GetDocReader(tempPdfPath, new PageDimensions(2160, 3840)))
            {
                for (int i = 0; i < docReader.GetPageCount(); i++)
                {
                    using (var pageReader = docReader.GetPageReader(i))
                    {
                        var rawBytes = pageReader.GetImage();
                        var width = pageReader.GetPageWidth();
                        var height = pageReader.GetPageHeight();

                        var pageText = OcrImage(rawBytes, width, height);

                        result.PageTexts.Add(new PageText
                        {
                            PageNumber = i + 1,
                            Text = pageText
                        });
                    }
                }
            }

            result.Success = true;
            result.FullText = string.Join("\n\n--- Page Break ---\n\n",
                result.PageTexts.Select(p => $"Page {p.PageNumber}:\n{p.Text}"));
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            if (File.Exists(tempPdfPath))
            {
                File.Delete(tempPdfPath);
            }
        }

        return result;
    }

    private string OcrImage(byte[] imageBytes, int width, int height)
    {
        try
        {
            using (var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default))
            {
                // Create bitmap from BGRA bytes
                var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                var pixelPtr = bitmap.GetPixels();
                System.Runtime.InteropServices.Marshal.Copy(imageBytes, 0, pixelPtr, imageBytes.Length);

                // Convert to PNG bytes for Tesseract
                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    var pngBytes = data.ToArray();

                    using (var pix = Pix.LoadFromMemory(pngBytes))
                    {
                        using (var page = engine.Process(pix))
                        {
                            return page.GetText();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error during OCR: {ex.Message}";
        }
    }
}

public class OcrResult
{
    public bool Success { get; set; }
    public string FullText { get; set; } = string.Empty;
    public List<PageText> PageTexts { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
    public string CleanedText { get; set; } = string.Empty;
}

public class PageText
{
    public int PageNumber { get; set; }
    public string Text { get; set; } = string.Empty;
}