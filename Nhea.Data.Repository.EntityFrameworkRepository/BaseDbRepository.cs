using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Nhea.Helper;

namespace Nhea.Data.Repository.EntityFrameworkRepository
{
    public abstract class BaseDbRepository<T> : Nhea.Data.Repository.EntityFrameworkRepository.BaseEntityFrameworkRepository<T>, IDisposable where T : class, new()
    {
        protected abstract System.Data.Entity.DbContext CurrentDbContext { get; }

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

        public override bool IsNew(T entity)
        {
            if (CurrentDbContext.Entry(entity).State == System.Data.Entity.EntityState.Added)
            {
                return true;
            }

            return false;
        }

        public override T GetById(object id)
        {
            try
            {
                System.Data.Entity.Core.Metadata.Edm.EdmMember key = CurrentObjectSet.EntitySet.ElementType.KeyMembers.SingleOrDefault();

                if (key != null)
                {
                    CurrentContext.MetadataWorkspace.LoadFromAssembly(typeof(T).Assembly);

                    System.Data.Entity.Core.EntityKey e = new System.Data.Entity.Core.EntityKey(this.CurrentContext.DefaultContainerName + "." + typeof(T).Name, key.Name, ConvertionHelper.GetConvertedValue(id, ((System.Data.Entity.Core.Metadata.Edm.PrimitiveType)(key.TypeUsage.EdmType)).ClrEquivalentType));

                    return (T)this.CurrentContext.GetObjectByKey(e);
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
