[![NuGet](https://img.shields.io/nuget/v/Nhea.Data.Repository.EntityFrameworkRepository.svg)](https://www.nuget.org/packages/Nhea.Data.Repository.EntityFrameworkRepository/)

# Nhea Entity Framework Repository

Nhea base repository classes for EF6


## Getting Started

Nhea is on NuGet. You may install Nhea Entity Framework Repository via NuGet Package manager.

https://www.nuget.org/packages/Nhea.Data.Repository.EntityFrameworkRepository/

### Prerequisites

Project is built with .NET Framework 4.6.1. 

### Configuration

Creating a base repository class is a good idea to initalize basic settings.

```
public abstract class Repository<T> : Nhea.Data.Repository.EntityFrameworkRepository.BaseDbRepository<T> where T : class, new()
    {
        private System.Data.Entity.DbContext currentDbContext;
        protected override System.Data.Entity.DbContext CurrentDbContext
        {
            get
            {
                if (currentDbContext == null)
                {
                    currentDbContext = new MyProject.MyEntities(); //Change with your context!
                    currentDbContext.Configuration.AutoDetectChangesEnabled = true;
                    currentDbContext.Configuration.LazyLoadingEnabled = true;
                    currentDbContext.Configuration.ProxyCreationEnabled = true;
                }

                return currentDbContext;
            }
        }
    }
```

You may remove the abstract modifier if you want to use generic repositories or you may create individual repository classes for your tables if you want to set specific properties for that table.

```
public class MemberRepository : BaseSqlRepository<Member>
    {
        public override Member CreateNew()
        {
            var entity = base.CreateNew();
            entity.Id = Guid.NewGuid();
            entity.CreateDate = DateTime.Now;
            entity.Status = (int)StatusType.Available;

            return entity;
        }

        public override Expression<Func<Member, object>> DefaultSorter => query => new { query.CreateDate };

        protected override SortDirection DefaultSortType => SortDirection.Descending;

        public override Expression<Func<Member, bool>> DefaultFilter => query => query.Status == (int)StatusType.Available;
    }
```

Then in your code just initalize a new instance of your class and call appropriate methods you need.

```
using (MemberRepository memberRepository = new MemberRepository())
{
    var member = memberRepository.CreateNew();
    member.Title = "Test Member";
    member.UserName = "username";
    member.Password = "password";
    member.Email = "test@test.com";
    memberRepository.Save();
}

using (MemberRepository memberRepository = new MemberRepository())
{
    var members = memberRepository.GetAll(query => query.CreateDate >= DateTime.Today).ToList();

    foreach (var member in members)
    {
        member.Title += " Lastname";
    }

    memberRepository.Save();
}

using (MemberRepository memberRepository = new MemberRepository())
{
    var member = memberRepository.GetById(new Guid("4D33AB34-5C5C-4A03-AF18-5C5FE6FC121A"));
    member.Title = "Selected Member";
    memberRepository.Save();
}

using (MemberRepository memberRepository = new MemberRepository())
{
    var member = memberRepository.GetSingle(query => query.Title == "Selected Member");
    member.Title = "Selected Member 2";
    memberRepository.Save();
}

using (MemberRepository memberRepository = new MemberRepository())
{
    memberRepository.Delete(query => query.Title == "Selected Member 2");
    memberRepository.Save();
}

using (MemberRepository memberRepository = new MemberRepository())
{
    var member = memberRepository.CreateNew();
    bool isNew = memberRepository.IsNew(member);
}
```