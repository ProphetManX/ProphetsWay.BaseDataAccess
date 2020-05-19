using ProphetsWay.BaseDataAccess.Example.Entities;
using ProphetsWay.BaseDataAccess.Example.ImplementorProject;
using FluentAssert;
using Xunit;
using System.Linq;
using ProphetsWay.BaseDataAccess.Example;
using System.Collections.Generic;
using System;

namespace ProphetsWay.BaseDataAccess.Tests
{
    public class CompanyDaoTests
    {
        IExampleDataAccess _da;

        public CompanyDaoTests()
        {
            //single instantiation of the data access class, makes for easy changing to test a separate implementation
            _da = new ExampleDataAccess();
            //_da = new NewDatabaseSourceDataAccess();
        }

        [Fact]
        public void ShouldInsertCompany()
        {
            //setup
            var co = new Company { Name = $"Bob {Guid.NewGuid()}" };

            //act
            _da.Insert(co);

            //assert
            co.Id.ShouldNotBeEqualTo(default);
        }

        public delegate void GetAssertion(Company co);
        public static (int CompanyId, GetAssertion Assertion) SetupShouldGetCompany(IExampleDataAccess da)
        {
            var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
            da.Insert(co);

            return(co.Id, (Company co2) => {
                co2.Name.ShouldBeEqualTo(co.Name);
            });
        }

        [Fact]
        public void ShouldGetCompany()
        {
            //setup
            var t = SetupShouldGetCompany(_da);

            //act
            var co2 = _da.Get(new Company { Id = t.CompanyId });

            //assert
            t.Assertion(co2);
        }

        [Fact]
        public void ShouldUpdateCompany()
        {
            //setup
            const string editText = "blarg";
            var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
            _da.Insert(co);

            //act
            co.Other = editText;
            var count = _da.Update(co);
            var co2 = _da.Get(co);

            //assert
            count.ShouldBeEqualTo(1);
            co2.Other.ShouldBeEqualTo(editText);

        }

        [Fact]
        public void ShouldDeleteCompany()
        {
            //setup
            var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
            _da.Insert(co);

            //act
            var count = _da.Delete(co);
            var co2 = _da.Get(co);

            //assert
            count.ShouldBeEqualTo(1);
            co2.ShouldBeNull();
        }

        public delegate void CountAssertion(int count);
        public static CountAssertion SetupShouldGetCount(IExampleDataAccess da)
        {
            var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
            da.Insert(co);
            var co1 = new Company { Name = $"Sam {Guid.NewGuid()}" };
            da.Insert(co1);
            var co2 = new Company { Name = $"Jim {Guid.NewGuid()}" };
            da.Insert(co2);

            return (int count) =>
            {
                count.ShouldBeGreaterThanOrEqualTo(3);
            };
        }

        [Fact]
        public void ShouldGetCount()
        {
            //setup
            var assertion = SetupShouldGetCount(_da);

            //act
            var count = _da.GetCount(new Company());

            //assert
            assertion(count);
        }

        public delegate void PagedAssertion(int count, IList<Company> all, IList<Company> subset);
        public static PagedAssertion SetupShouldGetPagedView(IExampleDataAccess da)
        {
            var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
            da.Insert(co);
            var co1 = new Company { Name = $"Sam {Guid.NewGuid()}" };
            da.Insert(co1);
            var co2 = new Company { Name = $"Jim {Guid.NewGuid()}" };
            da.Insert(co2);

            return (int count, IList<Company> all, IList<Company> subset) =>
            {
                all.Count.ShouldBeEqualTo(count);
                subset.Count.ShouldBeEqualTo(1);
                subset.First().Id.ShouldBeEqualTo(all.Skip(1).First().Id);
            };
        }

        [Fact]
        public void ShouldGetPagedView()
        {
            //setup
            var assertion = SetupShouldGetPagedView(_da);

            //act
            var count = _da.GetCount(new Company());
            var view = _da.GetPaged(new Company(), 0, count);
            var subset = _da.GetPaged(new Company(), 1, 1);

            //assert
            assertion(count, view, subset);            
        }

        [Fact]
        public void ShouldGetCustomCompanyFunction()
        {
            //setup
            var co = new Company { Name = $"Bob {Guid.NewGuid()}" };
            _da.Insert(co);
            var co1 = new Company { Name = $"Sam {Guid.NewGuid()}" };
            _da.Insert(co1);
            var co2 = new Company { Name = $"Jim {Guid.NewGuid()}" };
            _da.Insert(co2);

            //act
            var custom = _da.GetCustomCompanyFunction(100);

            //assert
            custom.ShouldNotBeNull();
        }
    }
}
