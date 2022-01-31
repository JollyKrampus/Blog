using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace BlazorBlogsLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment environment;

        public FileController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        [HttpGet("[action]")]
        public IActionResult DownloadFile(string FileName)
        {
            var path = Path.Combine(
                environment.WebRootPath,
                "files",
                FileName);

            var stream = new FileStream(path, FileMode.Open);

            var result = new FileStreamResult(stream, "text/plain");
            result.FileDownloadName = FileName;
            return result;
        }

        // This cannot be called by an API call
        public void DeleteFile(string FileName)
        {
            var path = Path.Combine(
                environment.WebRootPath,
                "files",
                FileName);

            if (System.IO.File.Exists(path))
                // If file found, delete it    
                System.IO.File.Delete(path);
        }
    }
}