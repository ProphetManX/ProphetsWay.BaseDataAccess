using ProphetsWay.BaseDataAccess.Example.Entities;
using ProphetsWay.BaseDataAccess.Example.ImplementorProject;
using FluentAssert;
using Xunit;
using ProphetsWay.BaseDataAccess.Example;
using System;

namespace ProphetsWay.BaseDataAccess.Tests
{
    public class UserDaoTests
    {
        IExampleDataAccess _da;

        public UserDaoTests()
        {
            //single instantiation of the data access class, makes for easy changing to test a separate implementation
            _da = new ExampleDataAccess();
            //_da = new NewDatabaseSourceDataAccess();
        }

        [Fact]
        public void ShouldInsertUser()
        {
            //setup
            var co = new User { Name = $"Bob {Guid.NewGuid()}" };

            //act
            _da.Insert(co);

            //assert
            co.Id.ShouldNotBeEqualTo(default);
        }

        public delegate void GetAssertion(User co);
        public static (int UserId, GetAssertion Assertion) SetupShouldGetUser(IExampleDataAccess da)
        {
            var co = new User { Name = $"Bob {Guid.NewGuid()}" };
            da.Insert(co);

            return (co.Id, (User co2) =>
            {
                co2.Name.ShouldBeEqualTo(co.Name);
            }
            );
        }

        [Fact]
        public void ShouldGetUser()
        {
            //setup
            var t = SetupShouldGetUser(_da);

            //act
            var co2 = _da.Get(new User { Id = t.UserId });

            //assert
            t.Assertion(co2);
        }

        [Fact]
        public void ShouldUpdateUser()
        {
            //setup
            const string editText = "blarg";
            var co = new User { Name = $"Bob {Guid.NewGuid()}" };
            _da.Insert(co);

            //act
            co.Whatever = editText;
            var count = _da.Update(co);
            var co2 = _da.Get(co);

            //assert
            count.ShouldBeEqualTo(1);
            co2.Whatever.ShouldBeEqualTo(editText);

        }

        [Fact]
        public void ShouldDeleteUser()
        {
            //setup
            var co = new User { Name = $"Bob {Guid.NewGuid()}" };
            _da.Insert(co);

            //act
            var count = _da.Delete(co);
            var co2 = _da.Get(co);

            //assert
            count.ShouldBeEqualTo(1);
            co2.ShouldBeNull();
        }

        [Fact]
        public void ShouldGetCustomFunctionality()
        {
            //setup
            var co = new User { Name = $"Eric {Guid.NewGuid()}" };
            _da.Insert(co);
            var currWhatever = co.Whatever;

            //act
            _da.CustomUserFunctionality(co);
            var co2 = _da.Get(co);

            //assert
            co2.Id.ShouldBeEqualTo(co.Id);
            co2.Whatever.ShouldNotBeEqualTo(currWhatever);
            co2.Whatever.ShouldBeEqualTo("custom functionality triggered");
        }

    }
}