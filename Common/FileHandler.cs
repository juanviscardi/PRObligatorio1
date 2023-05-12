namespace Common
{
    /*public class FileHandler
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string GetFileName(string path)
        {
            if (FileExists(path))
            {
                return new FileInfo(path).Name;
            }

            throw new Exception("File does not exist");
        }

        public long GetFileSize(string path)
        {
            if (FileExists(path))
            {
                return new FileInfo(path).Length;
            }

            throw new Exception("File does not exist");
        }
    }*/

    public class FileHandler
    {
        public async Task<bool> FileExists(string path)
        {
            return await Task.Run(() => File.Exists(path));
        }

        public async Task<string> GetFileName(string path)
        {
            if (await FileExists(path))
            {
                return new FileInfo(path).Name;
            }

            throw new Exception("File does not exist");
        }

        public async Task<long> GetFileSize(string path)
        {
            if (await FileExists(path))
            {
                return new FileInfo(path).Length;
            }

            throw new Exception("File does not exist");
        }
    }
}