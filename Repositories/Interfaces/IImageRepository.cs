namespace Pazaryeri.Repositories.Interfaces
{
    public interface IImageRepository
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<List<string>> UploadImagesAsync(List<IFormFile> files);
        Task<bool> DeleteImageAsync(string imageUrl);
    }
}
