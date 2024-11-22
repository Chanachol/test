using Microsoft.AspNetCore.Mvc;
using System.IO;

[Route("[controller]")]
public class DownloadController : Controller
{
    [HttpGet]
    public IActionResult Index(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        string fileName = Path.GetFileName(filePath);
        string mimeType = "application/octet-stream"; 
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        return File(fileStream, mimeType, fileName);
    }
}
