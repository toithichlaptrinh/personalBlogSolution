using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace personalBlogSolution.Services.Common
{
    public class FileStorageService: IStorageService
    {
        private readonly string _userContentFolder;
        private const string UserContentFolderName = "user-content";

        public FileStorageService(IWebHostEnvironment webHostEnvironment)
        {
            _userContentFolder = Path.Combine(webHostEnvironment.WebRootPath, UserContentFolderName);
        }

        public string GetFileUrl(string fileName)
        {
            return $"/{UserContentFolderName}/{fileName}";
        }

        public async Task SaveFileAsync(Stream mediaBinaryStream, string fileName)
        {
            var filePath = Path.Combine(_userContentFolder, fileName);
            var output = new FileStream(filePath, FileMode.Create);
            await mediaBinaryStream.CopyToAsync(output);
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var filePath = Path.Combine(_userContentFolder, fileName);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }
        }
    }
}