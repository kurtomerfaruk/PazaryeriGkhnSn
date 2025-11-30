using Pazaryeri.Repositories.Interfaces;

namespace Pazaryeri.Repositories
{
    public class ImageRepository:IImageRepository
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public ImageRepository(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Dosya boş olamaz.");
            }

            // Dosya uzantısını kontrol et
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException("Sadece JPG, JPEG, PNG ve GIF dosyaları yüklenebilir.");
            }

            // Dosya boyutunu kontrol et (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException("Dosya boyutu 5MB'tan büyük olamaz.");
            }

            // Uploads klasörünü oluştur
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Benzersiz dosya adı oluştur
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Dosyayı kaydet
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // URL'yi döndür
            return $"/uploads/products/{fileName}";
        }

        public async Task<List<string>> UploadImagesAsync(List<IFormFile> files)
        {
            var uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var url = await UploadImageAsync(file);
                    uploadedUrls.Add(url);
                }
            }

            return uploadedUrls;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return false;
            }

            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> ImportImageFromUrlAsync(string imageUrl)
        {
            var formFile = await DownloadImageAsFormFileAsync(imageUrl);

            var uploadedPath = await UploadImageAsync(formFile);

            return uploadedPath; 
        }

        private async Task<IFormFile> DownloadImageAsFormFileAsync(string imageUrl, string fileName = null)
        {
            using var httpClient = new HttpClient();
            var bytes = await httpClient.GetByteArrayAsync(imageUrl);

            var stream = new MemoryStream(bytes);

            string extension = Path.GetExtension(imageUrl);
            fileName ??= Guid.NewGuid() + extension;

            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = GetContentType(extension)
            };
        }

        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}
