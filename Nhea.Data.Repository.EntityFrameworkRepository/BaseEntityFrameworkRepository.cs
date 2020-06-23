using System;
using System.Collections.Generic;
using System.Linq;
using Nhea.Data;
using System.Linq.Expressions;
using System.Data.Entity.Core.Objects;
using System.Threading.Tasks;
using System.Data.Entity;
using Nhea.Helper;
using System.Data.Entity.Core;

namespace Nhea.Data.Repository.EntityFrameworkRepository
{
    public abstract class BaseEntityFrameworkRepository<T> : Nhea.Data.Repository.BaseRepository<T>, IDisposable where T : class, new()
    {
        protected abstract System.Data.Entity.DbContext CurrentDbContext { get; }

        protected abstract System.Data.Entity.Core.Objects.ObjectContext CurrentContext { get; }

        private System.Data.Entity.Core.Objects.ObjectContext CurrentContextWithOptions
        {
            get
            {
                var context = CurrentContext;

                if (this.IsReadOnly)
                {
                    CurrentDbContext.Configuration.AutoDetectChangesEnabled = false;
                    CurrentDbContext.Configuration.ProxyCreationEnabled = false;
                    context.ContextOptions.ProxyCreationEnabled = false;
                }

                return context;
            }
        }

        private ObjectSet<T> currentObjectSet;
        protected ObjectSet<T> CurrentObjectSet
        {
            get
            {
                if (currentObjectSet == null)
                {
                    currentObjectSet = CurrentContextWithOptions.CreateObjectSet<T>();
                }

                return currentObjectSet;
            }
            set
            {
                currentObjectSet = value;
            }
        }

        #region GetSingle

        protected override T GetSingleCore(Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            T entity = default(T);

            if (getDefaultFilter && DefaultFilter != null)
            {
                filter = filter.And(DefaultFilter);
            }

            if (filter != null)
            {
                entity = CurrentObjectSet.SingleOrDefault(filter);
            }

            return entity;
        }

        protected override async Task<T> GetSingleCoreAsync(Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            T entity = default(T);

            if (getDefaultFilter && DefaultFilter != null)
            {
                filter = filter.And(DefaultFilter);
            }

            if (filter != null)
            {
                entity = await CurrentObjectSet.SingleOrDefaultAsync(filter);
            }

            return entity;
        }

        #endregion

        #region GetAll

        protected override IQueryable<T> GetAllCore(Expression<Func<T, bool>> filter, bool getDefaultFilter, bool getDefaultSorter, string sortColumn, SortDirection? sortDirection, bool allowPaging, int pageSize, int pageIndex, ref int totalCount)
        {
            if (getDefaultFilter)
            {
                filter = filter.And(this.DefaultFilter);
            }

            if (filter == null)
            {
                filter = query => true;
            }

            IQueryable<T> returnList = CurrentObjectSet.Where(filter);

            if (!String.IsNullOrEmpty(sortColumn))
            {
                returnList = returnList.OrderBy(sortColumn + " " + sortDirection.ToString());
            }
            else if (getDefaultSorter && DefaultSorter != null)
            {
                if (DefaultSortType == SortDirection.Ascending)
                {
                    returnList = returnList.OrderBy(DefaultSorter);
                }
                else
                {
                    returnList = returnList.OrderByDescending(DefaultSorter);
                }
            }

            if (allowPaging && pageSize > 0)
            {
                if (totalCount == 0)
                {
                    totalCount = returnList.Count();
                }

                int skipCount = pageSize * pageIndex;

                returnList = returnList.Skip<T>(skipCount).Take<T>(pageSize);
            }

            return returnList;
        }

        #endregion

        private Expression<Func<T, bool>> SetFilter(Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            if (getDefaultFilter && DefaultFilter != null)
            {
                filter = filter.And(DefaultFilter);
            }

            if (filter == null)
            {
                filter = query => true;
            }

            return filter;
        }

        #region Count & Any

        protected override int CountCore(Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            return this.CurrentObjectSet.Count(SetFilter(filter, getDefaultFilter));
        }

        protected override async Task<int> CountCoreAsync(Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            return await this.CurrentObjectSet.CountAsync(SetFilter(filter, getDefaultFilter));
        }

        protected override bool AnyCore(Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            return this.CurrentObjectSet.Any(SetFilter(filter, getDefaultFilter));
        }

        protected override async Task<bool> AnyCoreAsync(Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            return await this.CurrentObjectSet.AnyAsync(SetFilter(filter, getDefaultFilter));
        }

        #endregion

        #region Add

        public override void Add(T entity)
        {
            CurrentObjectSet.AddObject(entity);
        }

        public override void Add(List<T> entities)
        {
            foreach (T entity in entities)
            {
                CurrentObjectSet.AddObject(entity);
            }
        }

        #endregion

        #region Save

        public override void Save()
        {
            CurrentContextWithOptions.SaveChanges();
        }

        public override async Task SaveAsync()
        {
            await CurrentContextWithOptions.SaveChangesAsync();
        }

        #endregion

        #region Delete

        public override void Delete(Expression<Func<T, bool>> filter)
        {
            var entites = CurrentObjectSet.Where(filter);

            foreach (var entity in entites)
            {
                CurrentContextWithOptions.DeleteObject(entity);
            }
        }

        public override void Delete(T entity)
        {
            CurrentContextWithOptions.DeleteObject(entity);
        }

        #endregion

        #region Refresh

        public override void Refresh(T entity)
        {
            CurrentContextWithOptions.Refresh(RefreshMode.StoreWins, entity);
        }

        #endregion

        public override void Dispose()
        {
            try
            {
                if (CurrentContext != null)
                {
                    CurrentContext.Dispose();
                }
            }
            catch
            { }

            try
            {
                if (CurrentObjectSet != null)
                {
                    CurrentObjectSet = null;
                }
            }
            catch
            { }

            try
            {
                if (CurrentDbContext != null)
                {
                    CurrentDbContext.Dispose();
                }
            }
            catch
            { }
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
                else
                {
                    throw new Exception("This object doesn't have a key specified!");
                }
            }
            catch (ObjectNotFoundException)
            {
                return null;
            }
            catch
            {
                throw;
            }
        }

        public override async Task<T> GetByIdAsync(object id)
        {
            return GetById(id);
        }

        public override bool IsNew(T entity)
        {
            return CurrentDbContext.Entry(entity).State == System.Data.Entity.EntityState.Added;
        }
    }
}