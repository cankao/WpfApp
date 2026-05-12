using System.Collections.Generic;

namespace WpfApp.Services
{
    public abstract class ServiceBase<T>
    {
        protected readonly JsonRepository<T> _repo;
        protected List<T> _cache;

        protected ServiceBase(string fileName)
        {
            _repo = new JsonRepository<T>(fileName);
            _cache = _repo.Load();
        }
    }
}
