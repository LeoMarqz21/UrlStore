namespace UrlStore.Services
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor httpContextAccessor;

        public LocalFileStorage(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            this.env = env;
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task Delete(string path, string directory)
        {
            if(path != null)
            {
                var filename = Path.GetFileName(path);
                var filePath = Path.Combine(env.WebRootPath, directory, filename);
                if(File.Exists(filePath)) File.Delete(filePath);
            }
            return Task.FromResult(0);
        }

        public async Task<string> Save(byte[] content, string extension, string directory, string contentType)
        {
            var filename = $"{Guid.NewGuid()}{extension}";
            var path = Path.Combine(env.WebRootPath, directory);
            if(!Directory.Exists(path)) Directory.CreateDirectory(path);
            var filePath = Path.Combine(path, filename);
            await File.WriteAllBytesAsync(filePath, content);
            var urlBase = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
            var urlDB = Path.Combine(urlBase, directory, filename).Replace("\\", "/");
            return urlDB;
        }

        public async Task<string> Update(string path, byte[] content, string extension, string directory, string contentType)
        {
            await Delete(path, directory);
            return await Save(content, extension, directory, contentType);
        }
    }
}
