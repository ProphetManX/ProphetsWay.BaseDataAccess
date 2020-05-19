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
    public class JobDaoTests
    {
        IExampleDataAccess _da;

        public JobDaoTests()
        {
            //single instantiation of the data access class, makes for easy changing to test a separate implementation
            _da = new ExampleDataAccess();
            //_da = new NewDatabaseSourceDataAccess();
        }

        [Fact]
        public void ShouldInsertJob()
        {
            //setup
            var co = new Job { Name = "Bob" };

            //act
            _da.Insert(co);

            //assert
            co.Id.ShouldNotBeEqualTo(default);
        }

        public delegate void GetAssertion(Job co);
        public static (int JobId, GetAssertion Assertion) SetupShouldGetUser(IExampleDataAccess da)
        {
            var co = new Job { Name = "Bob" };
            da.Insert(co);

            return (co.Id, (Job co2) =>
            {
                co2.Name.ShouldBeEqualTo(co.Name);
            }
            );
        }

        [Fact]
        public void ShouldGetJob()
        {
            //setup
            var t = SetupShouldGetUser(_da);

            //act
            var co2 = _da.Get(new Job { Id = t.JobId });

            //assert
            t.Assertion(co2);
        }

        [Fact]
        public void ShouldUpdateJob()
        {
            //setup
            const string editText = "blarg";
            var co = new Job { Name = "Bob" };
            _da.Insert(co);

            //act
            co.Name = editText;
            var count = _da.Update(co);
            var co2 = _da.Get(co);

            //assert
            count.ShouldBeEqualTo(1);
            co2.Name.ShouldBeEqualTo(editText);

        }

        [Fact]
        public void ShouldDeleteJob()
        {
            //setup
            var co = new Job { Name = "Bob" };
            _da.Insert(co);

            //act
            var count = _da.Delete(co);
            var co2 = _da.Get(co);

            //assert
            count.ShouldBeEqualTo(1);
            co2.ShouldBeNull();
        }

        [Fact]
        public void ShouldGetAllJobs()
        {
            //setup
            var assertion = SetupShouldGetAllJobs(_da);

            //act
            var all = _da.GetAll(new Job());

            //assert
            assertion(all);
        }

        public delegate void Assertion(IList<Job> all);
        public static Assertion SetupShouldGetAllJobs(IExampleDataAccess da)
        {
            var co = new Job { Name = $"Eric {Guid.NewGuid()}" };
            da.Insert(co);
            var co1 = new Job { Name = $"Sam {Guid.NewGuid()}" };
            da.Insert(co1);
            var co2 = new Job { Name = $"Jim {Guid.NewGuid()}" };
            da.Insert(co2);

            return (IList<Job> all) =>
            {
                all.Count.ShouldBeGreaterThanOrEqualTo(3);
                all.Where(x => x.Name == co.Name).Count().ShouldBeEqualTo(1);
                all.Where(x => x.Name == co1.Name).Count().ShouldBeEqualTo(1);
                all.Where(x => x.Name == co2.Name).Count().ShouldBeEqualTo(1);
            };
        }

    }
}
