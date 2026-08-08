using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// An exception raised by a derived method that was located successfully is not a convention failure. The
	/// caller must receive that exception itself — not a reflection wrapper around it — with its stack intact.
	/// </summary>
	public class BaseDataAccessExceptionPropagationTests
	{
		[Fact]
		public void ShouldPropagateTheOriginalExceptionFromGetAll()
		{
			//setup
			var dal = new ThrowingDataAccess();

			//act
			var ex = Should.Throw<DerivedMethodException>(() => dal.GetAll<Company>());

			//assert
			ex.Message.ShouldBe(ThrowingDataAccess.GetAllMessage);
		}

		[Fact]
		public void ShouldPropagateTheOriginalExceptionFromGetCount()
		{
			//setup
			var dal = new ThrowingDataAccess();

			//act
			var ex = Should.Throw<DerivedMethodException>(() => dal.GetCount<Company>());

			//assert
			ex.Message.ShouldBe(ThrowingDataAccess.GetCountMessage);
		}

		[Fact]
		public void ShouldPropagateTheOriginalExceptionFromGetPaged()
		{
			//setup
			var dal = new ThrowingDataAccess();

			//act
			var ex = Should.Throw<DerivedMethodException>(() => dal.GetPaged<Company>(0, 10));

			//assert
			ex.Message.ShouldBe(ThrowingDataAccess.GetPagedMessage);
		}

		[Fact]
		public void ShouldPropagateTheOriginalExceptionFromGet()
		{
			//setup
			var dal = new ThrowingDataAccess();

			//act
			var ex = Should.Throw<DerivedMethodException>(() => dal.Get<Company>(1));

			//assert
			ex.Message.ShouldBe(ThrowingDataAccess.GetMessage);
		}

		[Fact]
		public void ShouldPropagateTheOriginalExceptionFromInsert()
		{
			//setup
			var dal = new ThrowingDataAccess();

			//act
			var ex = Should.Throw<DerivedMethodException>(() => dal.Insert<Company>(new Company()));

			//assert
			ex.Message.ShouldBe(ThrowingDataAccess.InsertMessage);
		}

		[Fact]
		public void ShouldPropagateTheOriginalExceptionFromUpdate()
		{
			//setup
			var dal = new ThrowingDataAccess();

			//act
			var ex = Should.Throw<DerivedMethodException>(() => dal.Update<Company>(new Company()));

			//assert
			ex.Message.ShouldBe(ThrowingDataAccess.UpdateMessage);
		}

		[Fact]
		public void ShouldPropagateTheOriginalExceptionFromDelete()
		{
			//setup
			var dal = new ThrowingDataAccess();

			//act
			var ex = Should.Throw<DerivedMethodException>(() => dal.Delete<Company>(new Company()));

			//assert
			ex.Message.ShouldBe(ThrowingDataAccess.DeleteMessage);
		}

		[Fact]
		public void ShouldPreserveTheStackOfThePropagatedException()
		{
			//setup
			var dal = new ThrowingDataAccess();

			//act
			var ex = Should.Throw<DerivedMethodException>(() => dal.Insert<Company>(new Company()));

			//assert
			ex.StackTrace.ShouldNotBeNullOrEmpty();
			ex.StackTrace.ShouldContain(nameof(ThrowingDataAccess));
			ex.StackTrace.ShouldContain("Insert");
		}

		[Fact]
		public void ShouldPropagateAnEntityConstructorExceptionUnwrappedFromGet()
		{
			//setup
			var dal = new DetonatorDataAccess();

			//act
			var ex = Should.Throw<EntityConstructorException>(() => dal.Get<Detonator>(1));

			//assert
			ex.Message.ShouldBe("Detonator constructor refused to run.");
			dal.GetWasCalled.ShouldBeFalse();
		}
	}
}
