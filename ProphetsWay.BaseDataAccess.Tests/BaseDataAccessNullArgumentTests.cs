using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// <see cref="BaseDataAccess"/> does not guard against <c>null</c>. A null argument is forwarded to the
	/// derived Data Access Layer, which decides what to do with it, and any framework exception that results
	/// surfaces on its own terms rather than as a <see cref="DataAccessConventionException"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every call below supplies the type argument explicitly. Without it the compiler binds to the derived
	/// non-generic overload and the reflection path under test is never exercised.
	/// </para>
	/// <para>
	/// Where the identifier property can legitimately hold <c>null</c> — a reference type or a nullable value
	/// type — the whole path is asserted end to end. Where it cannot, the outcome is decided by the CLR's
	/// reflection layer rather than by this library, so only the part this library owns is asserted.
	/// </para>
	/// </remarks>
	public class BaseDataAccessNullArgumentTests
	{
		[Fact]
		public void ShouldForwardANullItemToTheDerivedInsertMethod()
		{
			//setup
			var dal = new WellFormedDataAccess();

			//act
			dal.Insert<Company>(null);

			//assert
			dal.InsertWasCalled.ShouldBeTrue();
			dal.InsertItem.ShouldBeNull();
		}

		[Fact]
		public void ShouldForwardANullItemToTheDerivedUpdateMethodAndReturnItsRowCount()
		{
			//setup
			var dal = new WellFormedDataAccess();
			dal.UpdateResult = 4;

			//act
			var result = dal.Update<Company>(null);

			//assert
			dal.UpdateWasCalled.ShouldBeTrue();
			dal.UpdateItem.ShouldBeNull();
			result.ShouldBe(4);
		}

		[Fact]
		public void ShouldForwardANullItemToTheDerivedDeleteMethodAndReturnItsRowCount()
		{
			//setup
			var dal = new WellFormedDataAccess();
			dal.DeleteResult = 2;

			//act
			var result = dal.Delete<Company>(null);

			//assert
			dal.DeleteWasCalled.ShouldBeTrue();
			dal.DeleteItem.ShouldBeNull();
			result.ShouldBe(2);
		}

		[Fact]
		public void ShouldNotReportAConventionFailureWhenGetIsGivenANullIdForAValueTypeIdProperty()
		{
			//setup
			var dal = new WellFormedDataAccess();

			//act
			var ex = Record.Exception(() => dal.Get<Company>(null));

			//assert
			//writing null to a non-nullable value-type property is resolved by the CLR's reflection layer, so
			//whether it throws at all is not this library's to promise; that it is never a convention failure is
			(ex as DataAccessConventionException).ShouldBeNull();
		}

		[Fact]
		public void ShouldPassANullIdThroughToTheDerivedGetWhenTheIdPropertyIsAReferenceType()
		{
			//setup
			var dal = new NullIdDataAccess();

			//act
			var result = dal.Get<Ticket>(null);

			//assert
			dal.GetTicketWasCalled.ShouldBeTrue();
			dal.TicketProbe.ShouldNotBeNull();
			dal.TicketProbe.TicketId.ShouldBeNull();
			result.ShouldBeSameAs(dal.GetTicketResult);
		}

		[Fact]
		public void ShouldPassANullIdThroughToTheDerivedGetWhenTheIdPropertyIsANullableValueType()
		{
			//setup
			var dal = new NullIdDataAccess();

			//act
			var result = dal.Get<Coupon>(null);

			//assert
			dal.GetCouponWasCalled.ShouldBeTrue();
			dal.CouponProbe.ShouldNotBeNull();
			dal.CouponProbe.Id.ShouldBeNull();
			result.ShouldBeSameAs(dal.GetCouponResult);
		}
	}
}
