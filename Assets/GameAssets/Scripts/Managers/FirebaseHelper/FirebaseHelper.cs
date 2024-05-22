using System.Threading.Tasks;
using Firebase;

namespace com.tinycastle.StickerBooker
{
    public static class FirebaseHelper
    {
        private static Task<DependencyStatus> _dependencyTask;
        
        public static Task<DependencyStatus> CheckDependencies()
        {
            if (_dependencyTask == null)
            {
                _dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
            }

            return _dependencyTask;
        }
    }
}