using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Nhea.Helper;

namespace Nhea.Data.Repository.EntityFrameworkRepository
{
    public abstract class BaseDbRepository<T> : Nhea.Data.Repository.EntityFrameworkRepository.BaseEntityFrameworkRepository<T>, IDisposable where T : class, new()
    {
        private System.Data.Entity.Core.Objects.ObjectContext currentContext;
        protected override System.Data.Entity.Core.Objects.ObjectContext CurrentContext
        {
            get
            {
                if (currentContext == null)
                {
                    currentContext = ((IObjectContextAdapter)CurrentDbContext).ObjectContext;
                    currentContext.ContextOptions.LazyLoadingEnabled = true;
                    currentContext.ContextOptions.ProxyCreationEnabled = true;
                }

                return currentContext;
            }
        }
    }
}
