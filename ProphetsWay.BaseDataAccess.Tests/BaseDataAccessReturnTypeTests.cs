using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// A located method whose <i>declared</i> return type the calling <see cref="BaseDataAccess"/> member cannot
	/// hand back. Every member is guarded the same way, not only the collection-returning ones.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The guard is uniform and its timing is what the invocation probes pin down. The check is made against the
	/// return type the derived method <i>declares</i>, <i>before</i> the method is invoked, for reads and writes
	/// alike — so every probe in a wrong-return-type test must stay false. The value the method would have
	/// produced is never examined and its runtime type is irrelevant; a mis-declared <c>Update</c> or
	/// <c>Delete</c> therefore cannot write to the database and only then report the defect.
	/// </para>
	/// <para>
	/// <c>Insert</c> is the one exception, and it is exempt rather than deferred: its return value is discarded
	/// entirely, so no declared return type can violate the convention. That exemption is asserted here too, so
	/// that adding a guard to <c>Insert</c> breaks a test rather than passing unnoticed.
	/// </para>
	/// <para>
	/// Message assertions quote the offending member name and deny its siblings, for the reasons set out on
	/// <see cref="BaseDataAccessMethodLookupTests"/>.
	/// </para>
	/// </remarks>
	public class BaseDataAccessReturnTypeTests
	{
		[Fact]
		public void ShouldThrowConventionExceptionWhenGetAllDoesNotReturnAList()
		{
			//setup
			var dal = new WrongReturnTypeDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetAll<Company>());

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetAll'");
			ex.Message.ShouldNotContain("'Get'");
			ex.Message.ShouldNotContain("'GetPaged'");
			ex.Message.ShouldNotContain("'GetCount'");
			dal.GetAllWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenGetPagedDoesNotReturnAList()
		{
			//setup
			var dal = new WrongReturnTypeDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetPaged<Company>(0, 10));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetPaged'");
			ex.Message.ShouldNotContain("'Get'");
			ex.Message.ShouldNotContain("'GetAll'");
			ex.Message.ShouldNotContain("'GetCount'");
			dal.GetPagedWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenGetCountDoesNotReturnAnInt()
		{
			//setup
			var dal = new WrongReturnTypeDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetCount<Company>());

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetCount'");
			ex.Message.ShouldNotContain("'Get'");
			ex.Message.ShouldNotContain("'GetAll'");
			ex.Message.ShouldNotContain("'GetPaged'");
			dal.GetCountWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenUpdateDoesNotReturnAnInt()
		{
			//setup
			var dal = new WrongReturnTypeDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Update<Company>(new Company()));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'Update'");
			ex.Message.ShouldNotContain("'Insert'");
			ex.Message.ShouldNotContain("'Delete'");
			dal.UpdateWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenDeleteDoesNotReturnAnInt()
		{
			//setup
			var dal = new WrongReturnTypeDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Delete<Company>(new Company()));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'Delete'");
			ex.Message.ShouldNotContain("'Insert'");
			ex.Message.ShouldNotContain("'Update'");
			dal.DeleteWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenGetDoesNotReturnTheEntityType()
		{
			//setup
			var dal = new WrongReturnTypeDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Get<Company>(1));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'Get'");
			ex.Message.ShouldNotContain("GetAll");
			ex.Message.ShouldNotContain("GetPaged");
			ex.Message.ShouldNotContain("GetCount");
			dal.GetWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldAcceptAGetAllThatDeclaresAConcreteListReturnType()
		{
			//setup
			var dal = new CovariantReturnDataAccess();

			//act
			var result = dal.GetAll<Company>();

			//assert
			dal.GetAllWasCalled.ShouldBeTrue();
			result.ShouldBeSameAs(dal.GetAllResult);
			result.Count.ShouldBe(1);
		}

		[Fact]
		public void ShouldAcceptAGetPagedThatDeclaresAnArrayReturnType()
		{
			//setup
			var dal = new CovariantReturnDataAccess();

			//act
			var result = dal.GetPaged<Company>(0, 10);

			//assert
			dal.GetPagedWasCalled.ShouldBeTrue();
			result.ShouldBeSameAs(dal.GetPagedResult);
			result.Count.ShouldBe(1);
		}

		[Fact]
		public void ShouldAcceptAGetThatDeclaresADerivedEntityReturnType()
		{
			//setup
			var dal = new CovariantReturnDataAccess();

			//act
			var result = dal.Get<Company>(42);

			//assert
			dal.GetWasCalled.ShouldBeTrue();
			result.ShouldBeSameAs(dal.GetResult);
		}

		[Fact]
		public void ShouldIgnoreTheDeclaredReturnTypeOfInsert()
		{
			//setup
			var dal = new IgnoredInsertReturnDataAccess();
			var item = new Company { Name = "Acme" };

			//act
			var ex = Record.Exception(() => dal.Insert<Company>(item));

			//assert
			ex.ShouldBeNull();
			dal.InsertWasCalled.ShouldBeTrue();
			dal.InsertItem.ShouldBeSameAs(item);
		}
	}
}
