using System.Net.Http.Headers;

namespace DesignPatternsSingleton.UploadService;
sealed class UploadService
{
    private UploadService()
    {
        
    }
    public int Id{get;private set;}
    private static UploadService _instance;
    private static readonly object _lock = new object();
    public static UploadService Instance(int id)
    {
        if(_instance == null)
        {
            //if two thread or multiple thread reach this, only single thread proceed to it while other threads are waiting
            lock (_lock)
            {
                if(_instance == null)
                {    
                    _instance = new UploadService();
                    _instance.Id = id;
                }
            }
        }
        return _instance;
    }
}
