namespace UrlStore.Services
{
    public interface IFileStorage
    {
        Task<string> Save(byte[] content, string extension, string directory, string contentType);
        Task<string> Update(string path, byte[] content, string extension, string directory, string contentType);
        Task Delete(string path, string directory);
    }
}
