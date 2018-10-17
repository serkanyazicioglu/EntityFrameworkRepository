using System;
using System.Collections.Generic;
using System.Linq;
using Nhea.Data;
using System.Linq.Expressions;
using System.Data.Entity.Core.Objects;

namespace Nhea.Data.Repository.EntityFrameworkRepository
{
    public abstract class BaseEntityFrameworkRepository<T> : Nhea.Data.Repository.BaseRepository<T>, IDisposable where T : class, new()
    {
        protected abstract System.Data.Entity.Core.Objects.ObjectContext CurrentContext { get; }

        private ObjectSet<T> currentObjectSet;
        protected ObjectSet<T> CurrentObjectSet
        {
            get
            {
                if (currentObjectSet == null)
                {
                    currentObjectSet = CurrentContext.CreateObjectSet<T>();
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

        #region Count & Any

        protected override int CountCore(Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            if (getDefaultFilter && DefaultFilter != null)
            {
                filter = filter.And(DefaultFilter);
            }

            if (filter == null)
            {
                filter = query => true;
            }

            return this.CurrentObjectSet.Count(filter);
        }

        protected override bool AnyCore(Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            if (getDefaultFilter && DefaultFilter != null)
            {
                filter = filter.And(DefaultFilter);
            }

            if (filter == null)
            {
                filter = query => true;
            }

            return this.CurrentObjectSet.Any(filter);
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
            CurrentContext.SaveChanges();
        }

        #endregion

        #region Delete

        public override void Delete(Expression<Func<T, bool>> filter)
        {
            var entites = CurrentObjectSet.Where(filter);

            foreach (var entity in entites)
            {
                CurrentContext.DeleteObject(entity);
            }
        }

        public override void Delete(T entity)
        {
            CurrentContext.DeleteObject(entity);
        }

        #endregion

        #region Refresh

        public override void Refresh(T entity)
        {
            CurrentContext.Refresh(RefreshMode.StoreWins, entity);
        }

        #endregion

        public override void Dispose()
        {
            if (CurrentContext != null)
            {
                CurrentContext.Dispose();
            }

            if (CurrentObjectSet != null)
            {
                CurrentObjectSet = null;
            }
        }
    }
}