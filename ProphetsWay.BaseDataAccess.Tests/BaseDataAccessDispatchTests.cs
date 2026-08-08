using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// The happy path: a well-formed derived DAL is located, invoked with the documented arguments, and its
	/// return value is handed back to the caller unchanged.
	/// </summary>
	/// <remarks>
	/// Every call below supplies the type argument explicitly. Without it the compiler binds to the derived
	/// non-generic overload and the reflection path under test is never exercised.
	/// </remarks>
	public class BaseDataAccessDispatchTests
	{
		[Fact]
		public void ShouldDispatchGetAllToTheDerivedMethodAndReturnItsList()
		{
			//setup
			var dal = new WellFormedDataAccess();
			dal.GetAllResult = new List<Company> { new Company { Name = "Acme" } };

			//act
			var result = dal.GetAll<Company>();

			//assert
			dal.GetAllWasCalled.ShouldBeTrue();
			dal.GetAllProbe.ShouldBeNull();
			result.ShouldBeSameAs(dal.GetAllResult);
			result.Count.ShouldBe(1);
		}

		[Fact]
		public void ShouldDispatchGetCountToTheDerivedMethodAndReturnItsCount()
		{
			//setup
			var dal = new WellFormedDataAccess();
			dal.GetCountResult = 17;

			//act
			var result = dal.GetCount<Company>();

			//assert
			dal.GetCountWasCalled.ShouldBeTrue();
			dal.GetCountProbe.ShouldBeNull();
			result.ShouldBe(17);
		}

		[Fact]
		public void ShouldDispatchGetPagedToTheDerivedMethodAndReturnItsList()
		{
			//setup
			var dal = new WellFormedDataAccess();
			dal.GetPagedResult = new List<Company> { new Company { Name = "Acme" } };

			//act
			var result = dal.GetPaged<Company>(20, 5);

			//assert
			dal.GetPagedWasCalled.ShouldBeTrue();
			dal.GetPagedProbe.ShouldBeNull();
			result.ShouldBeSameAs(dal.GetPagedResult);
		}

		[Fact]
		public void ShouldPassSkipAndTakeToGetPagedInTheCorrectPositions()
		{
			//setup
			var dal = new WellFormedDataAccess();

			//act
			dal.GetPaged<Company>(20, 5);

			//assert
			dal.GetPagedSkip.ShouldBe(20);
			dal.GetPagedTake.ShouldBe(5);
		}

		[Fact]
		public void ShouldDispatchGetToTheDerivedMethodAndReturnItsEntity()
		{
			//setup
			var dal = new WellFormedDataAccess();
			dal.GetResult = new Company { CompanyId = 99, Name = "Returned" };

			//act
			var result = dal.Get<Company>(42);

			//assert
			dal.GetWasCalled.ShouldBeTrue();
			dal.GetProbe.ShouldNotBeNull();
			dal.GetProbe.CompanyId.ShouldBe(42);
			result.ShouldBeSameAs(dal.GetResult);
		}

		[Fact]
		public void ShouldDispatchInsertToTheDerivedMethodWithTheSuppliedItem()
		{
			//setup
			var dal = new WellFormedDataAccess();
			var item = new Company { Name = "Acme" };

			//act
			dal.Insert<Company>(item);

			//assert
			dal.InsertWasCalled.ShouldBeTrue();
			dal.InsertItem.ShouldBeSameAs(item);
		}

		[Fact]
		public void ShouldDispatchUpdateToTheDerivedMethodAndReturnItsRowCount()
		{
			//setup
			var dal = new WellFormedDataAccess();
			dal.UpdateResult = 3;
			var item = new Company { Name = "Acme" };

			//act
			var result = dal.Update<Company>(item);

			//assert
			dal.UpdateWasCalled.ShouldBeTrue();
			dal.UpdateItem.ShouldBeSameAs(item);
			result.ShouldBe(3);
		}

		[Fact]
		public void ShouldDispatchDeleteToTheDerivedMethodAndReturnItsRowCount()
		{
			//setup
			var dal = new WellFormedDataAccess();
			dal.DeleteResult = 1;
			var item = new Company { Name = "Acme" };

			//act
			var result = dal.Delete<Company>(item);

			//assert
			dal.DeleteWasCalled.ShouldBeTrue();
			dal.DeleteItem.ShouldBeSameAs(item);
			result.ShouldBe(1);
		}
	}
}
