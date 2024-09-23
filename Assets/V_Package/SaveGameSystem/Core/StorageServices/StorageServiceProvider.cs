using VPackage.SaveGameSystem.StorageServices;

namespace VPackage.SaveGameSystem
{
    public static class StorageServiceProvider
    {
        public static StorageService Generate()
        {
            return new StorageService_FileSplit();
        }
    }
}