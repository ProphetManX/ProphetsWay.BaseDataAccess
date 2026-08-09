using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// A derived method that returns <c>null</c> is well formed. <c>null</c> is the ordinary way a Data Access
	/// Layer says "no row" or "no result", and <see cref="DataAccessConventionException"/> documents itself as
	/// signalling a structural defect that "never indicates a runtime data condition: a missing row, an empty
	/// table, a null identifier, or a failed query will not produce it."
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is the distinction a type check phrased as <c>if (!(raw is T)) throw</c> gets wrong: <c>null</c>
	/// fails every <c>is</c> test, so such an implementation turns every miss into a convention exception.
	/// The check must be on the value's type when it has one, not on its mere presence.
	/// </para>
	/// <para>
	/// Every call below supplies the type argument explicitly. Without it the compiler binds to the derived
	/// non-generic overload and the reflection path under test is never exercised.
	/// </para>
	/// </remarks>
	public class BaseDataAccessNullResultTests
	{
		[Fact]
		public void ShouldReturnNullWhenTheDerivedGetFindsNoRow()
		{
			//setup
			var dal = new WellFormedDataAccess();
			dal.GetResult = null;

			//act
			var result = dal.Get<Company>(42);

			//assert
			dal.GetWasCalled.ShouldBeTrue();
			result.ShouldBeNull();
		}

		[Fact]
		public void ShouldReturnNullWhenTheDerivedGetAllReturnsNull()
		{
			//setup
			var dal = new WellFormedDataAccess();
			dal.GetAllResult = null;

			//act
			var result = dal.GetAll<Company>();

			//assert
			dal.GetAllWasCalled.ShouldBeTrue();
			result.ShouldBeNull();
		}

		[Fact]
		public void ShouldReturnNullWhenTheDerivedGetPagedReturnsNull()
		{
			//setup
			var dal = new WellFormedDataAccess();
			dal.GetPagedResult = null;

			//act
			var result = dal.GetPaged<Company>(0, 10);

			//assert
			dal.GetPagedWasCalled.ShouldBeTrue();
			result.ShouldBeNull();
		}
	}
}
