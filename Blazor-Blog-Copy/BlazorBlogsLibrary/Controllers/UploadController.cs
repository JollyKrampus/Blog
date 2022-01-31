using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using BlazorBlogs.Data;
using BlazorBlogsLibrary.Data;
using BlazorBlogsLibrary.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Version = BlazorBlogsLibrary.Classes.Version;

namespace BlazorBlogsLibrary.Controllers
{
    public class FileObject
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int ThumbnailHeight { get; set; }
        public int ThumbnailWidth { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    // to ensure that user must be in the Administrator Role
    [Authorize(Roles = "Administrators")]
    public class UploadController : Controller
    {
        private readonly BlazorBlogsContext blogsContext;
        private readonly IWebHostEnvironment environment;
        private readonly GeneralSettingsService generalSettingsService;
        private IHostApplicationLifetime applicationLifetime;

        public UploadController(IWebHostEnvironment environment,
            BlazorBlogsContext context,
            GeneralSettingsService generalSettingsService,
            IHostApplicationLifetime appLifetime)
        {
            this.environment = environment;
            blogsContext = context;
            this.generalSettingsService = generalSettingsService;
            applicationLifetime = appLifetime;
        }

        #region public async Task<IActionResult> MultipleAsync(IFormFile[] files, string CurrentDirectory)

        [HttpPost("[action]")]
        public async Task<IActionResult> MultipleAsync(
            IFormFile[] files, string CurrentDirectory)
        {
            try
            {
                if (HttpContext.Request.Form.Files.Any())
                    foreach (var file in HttpContext.Request.Form.Files)
                    {
                        // reconstruct the path to ensure everything 
                        // goes to uploads directory

                        if (CurrentDirectory == null) CurrentDirectory = "";

                        var RequestedPath =
                            CurrentDirectory.ToLower()
                                .Replace(environment.WebRootPath.ToLower(), "");

                        if (RequestedPath.Contains("\\uploads\\"))
                            RequestedPath =
                                RequestedPath.Replace("\\uploads\\", "");

                        // If RequestedPath begins with \\ remove them
                        if (RequestedPath.Length > 1)
                            if (RequestedPath.Substring(0, 1) == @"\")
                                RequestedPath = RequestedPath.Remove(0, 1);

                        var path =
                            Path.Combine(
                                environment.WebRootPath,
                                "uploads\\",
                                RequestedPath,
                                file.FileName);

                        using (var stream =
                               new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }

                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region public async Task<IActionResult> SingleAsync(IFormFile file, string FileTitle)

        [HttpPost("[action]")]
        public async Task<IActionResult> SingleAsync(
            IFormFile file, string FileTitle)
        {
            try
            {
                if (HttpContext.Request.Form.Files.Any())
                    // Only accept .zip files
                    if (file.ContentType == "application/x-zip-compressed")
                    {
                        var path =
                            Path.Combine(
                                environment.WebRootPath,
                                "files",
                                file.FileName);

                        // Create directory if not exists
                        var directoryName = Path.GetDirectoryName(path);
                        if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);

                        using (var stream =
                               new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Save to database
                        if (FileTitle == "") FileTitle = "[Unknown]";

                        var objFilesDTO = new FilesDTO();
                        objFilesDTO.FileName = FileTitle;
                        objFilesDTO.FilePath = file.FileName;

                        var objBlogsService = new BlogsService(blogsContext, environment);
                        await objBlogsService.CreateFilesAsync(objFilesDTO);
                    }

                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion

        #region public async Task<IActionResult> UpgradeAsync(IFormFile file, string FileTitle)

        [HttpPost("[action]")]
        public async Task<IActionResult> UpgradeAsync(
            IFormFile file, string FileTitle)
        {
            try
            {
                if (HttpContext.Request.Form.Files.Any())
                    // Only accept .zip files
                    if (file.ContentType == "application/x-zip-compressed")
                    {
                        var UploadPath =
                            Path.Combine(
                                environment.ContentRootPath,
                                "Uploads");

                        var UploadPathAndFile =
                            Path.Combine(
                                environment.ContentRootPath,
                                "Uploads",
                                "BlazorBlogsUpgrade.zip");

                        var UpgradePath = Path.Combine(
                            environment.ContentRootPath,
                            "Upgrade");

                        // Upload Upgrade package to Upload Folder
                        if (!Directory.Exists(UpgradePath)) Directory.CreateDirectory(UpgradePath);

                        using (var stream =
                               new FileStream(UploadPathAndFile, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        DeleteFiles(UpgradePath);

                        // Unzip files to Upgrade folder
                        ZipFile.ExtractToDirectory(UploadPathAndFile, UpgradePath, true);

                        #region Check upgrade - Get current version

                        var objVersion = new Version();
                        var GeneralSettings = await generalSettingsService.GetGeneralSettingsAsync();
                        objVersion.VersionNumber = GeneralSettings.VersionNumber;

                        #endregion

                        #region Examine the manifest file

                        objVersion = ReadManifest(objVersion, UpgradePath);

                        try
                        {
                            if (objVersion.ManifestLowestVersion == "")
                            {
                                // Delete the files
                                DeleteFiles(UpgradePath);
                                return Ok("Error: could not find manifest");
                            }
                        }
                        catch (Exception ex)
                        {
                            return Ok(ex.ToString());
                        }

                        #endregion

                        #region Show error if needed and delete upgrade files

                        if
                        (
                            ConvertToInteger(objVersion.VersionNumber) >
                            ConvertToInteger(objVersion.ManifestHighestVersion) ||
                            ConvertToInteger(objVersion.VersionNumber) <
                            ConvertToInteger(objVersion.ManifestLowestVersion)
                        )
                        {
                            // Delete the files
                            DeleteFiles(UpgradePath);

                            // Return the error response
                            return Ok(objVersion.ManifestFailure);
                        }

                        #endregion

                        // Proceed with upgrade...

                        DeleteFiles(UpgradePath);

                        // Unzip files to final paths
                        ZipFile.ExtractToDirectory(UploadPathAndFile, environment.ContentRootPath, true);

                        Task.Delay(4000).Wait(); // Wait 4 seconds with blocking
                    }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return Ok();
        }

        #endregion

        // Upgrade Code

        #region private static void DeleteFiles(string FilePath)

        private static void DeleteFiles(string FilePath)
        {
            if (Directory.Exists(FilePath))
            {
                var Directory = new DirectoryInfo(FilePath);
                Directory.Delete(true);
                Directory.Create();
            }
        }

        #endregion

        #region private Version ReadManifest(Version objVersion, string UpgradePath)

        private Version ReadManifest(Version objVersion, string UpgradePath)
        {
            string strManifest;
            var strFilePath = Path.Combine(UpgradePath, "Manifest.json");

            if (!System.IO.File.Exists(strFilePath))
            {
                // Manifest not found
                objVersion.ManifestLowestVersion = "";
                return objVersion;
            }

            using (var reader = new StreamReader(strFilePath))
            {
                strManifest = reader.ReadToEnd();
                reader.Close();
            }

            dynamic objManifest = JsonConvert.DeserializeObject(strManifest);

            objVersion.ManifestHighestVersion = objManifest.ManifestHighestVersion;
            objVersion.ManifestLowestVersion = objManifest.ManifestLowestVersion;
            objVersion.ManifestSuccess = objManifest.ManifestSuccess;
            objVersion.ManifestFailure = objManifest.ManifestFailure;

            return objVersion;
        }

        #endregion

        #region private int ConvertToInteger(string strParamVersion)

        private int ConvertToInteger(string strParamVersion)
        {
            var intVersionNumber = 0;
            var strVersion = strParamVersion;

            // Split into parts seperated by periods
            char[] splitchar = {'.'};
            var strSegments = strVersion.Split(splitchar);

            // Process the segments
            var i = 0;
            var colMultiplyers = new List<int> {10000, 100, 1};
            foreach (var strSegment in strSegments)
            {
                var intSegmentNumber = Convert.ToInt32(strSegment);
                intVersionNumber = intVersionNumber + intSegmentNumber * colMultiplyers[i];
                i++;
            }

            return intVersionNumber;
        }

        #endregion
    }
}