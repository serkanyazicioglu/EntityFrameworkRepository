using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{
    public abstract class BaseSqlRepository<T> : Nhea.Data.Repository.EntityFrameworkRepository.BaseDbRepository<T> where T : class, new()
    {
        private System.Data.Entity.DbContext currentDbContext;
        protected override System.Data.Entity.DbContext CurrentDbContext
        {
            get
            {
                if (currentDbContext == null)
                {
                    currentDbContext = new DataModel();
                    currentDbContext.Configuration.AutoDetectChangesEnabled = true;
                    currentDbContext.Configuration.LazyLoadingEnabled = true;
                    currentDbContext.Configuration.ProxyCreationEnabled = true;
                }

                return currentDbContext;
            }
        }
    }
}
