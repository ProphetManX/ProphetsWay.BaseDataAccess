using System;
using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// <see cref="BaseDataAccess"/> does not guard against <c>null</c> on <c>Insert</c>, <c>Update</c> or
	/// <c>Delete</c>: the null item is forwarded to the derived Data Access Layer, which decides what to do with
	/// it. <c>Get&lt;T&gt;(object id)</c> is the exception, because it must write the identifier onto a probe
	/// entity before there is anything to forward, so a null identifier travels only as far as the resolved
	/// identifier property can hold it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every call below supplies the type argument explicitly. Without it the compiler binds to the derived
	/// non-generic overload and the reflection path under test is never exercised.
	/// </para>
	/// <para>
	/// Whether a null identifier can be held is a question about the property's type alone. A reference type
	/// (<see cref="Ticket.TicketId"/>) and a nullable value type (<see cref="Coupon.Id"/>) can both hold one, so
	/// the null reaches the derived <c>Get</c> and the whole path is asserted end to end. A non-nullable value
	/// type (<see cref="Company.CompanyId"/>) cannot, and the library rejects that call itself, with an
	/// <see cref="ArgumentException"/> raised before reflection is reached. It is not left to the reflection
	/// layer, which is lenient about this one case and would write <c>default</c> instead of throwing, sending a
	/// probe for identifier zero out as though the caller had asked for it.
	/// </para>
	/// <para>
	/// The rejection is a caller error rather than a wiring error in the entity, so it is deliberately never a
	/// <see cref="DataAccessConventionException"/>. Its full contract belongs to
	/// <see cref="BaseDataAccessIdentifierRejectionTests"/>; it is stated again here so the three outcomes a null
	/// identifier can have — rejected, passed through as a reference type, passed through as a nullable value
	/// type — can be read as one story rather than two thirds of one.
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
		public void ShouldNotForwardANullIdToTheDerivedGetWhenTheIdPropertyIsANonNullableValueType()
		{
			//setup
			var dal = new WellFormedDataAccess();

			//act
			var ex = Should.Throw<ArgumentException>(() => dal.Get<Company>(null));

			//assert
			//a caller error, never a wiring error, and settled before anything could be queried
			ex.ShouldNotBeAssignableTo<DataAccessConventionException>();
			dal.GetWasCalled.ShouldBeFalse();
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
