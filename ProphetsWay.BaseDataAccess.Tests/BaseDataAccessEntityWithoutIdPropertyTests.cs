using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// An entity exposing neither <c>{TypeName}Id</c> nor <c>Id</c> is perfectly usable with the three members
	/// that never construct a probe entity.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>GetAll&lt;T&gt;()</c>, <c>GetCount&lt;T&gt;()</c> and <c>GetPaged&lt;T&gt;(int, int)</c> pass
	/// <c>null</c> as the entity argument, so identifier resolution never runs for them. Only
	/// <c>Get&lt;T&gt;(object)</c> needs an identifier property, and the failure of
	/// <see cref="Ghost"/> there is asserted in <see cref="BaseDataAccessIdResolutionTests"/>. These three
	/// guard the other direction: identifier resolution must not migrate into shared setup where it would
	/// break entity types that legitimately have no key property.
	/// </para>
	/// <para>
	/// Every call below supplies the type argument explicitly. Without it the compiler binds to the derived
	/// non-generic overload and the reflection path under test is never exercised.
	/// </para>
	/// </remarks>
	public class BaseDataAccessEntityWithoutIdPropertyTests
	{
		[Fact]
		public void ShouldDispatchGetAllForAnEntityWithNoIdentifierProperty()
		{
			//setup
			var dal = new NoIdPropertyDataAccess();

			//act
			var result = dal.GetAll<Ghost>();

			//assert
			dal.GetAllWasCalled.ShouldBeTrue();
			dal.GetAllProbe.ShouldBeNull();
			result.ShouldBeSameAs(dal.GetAllResult);
			result.Count.ShouldBe(1);
		}

		[Fact]
		public void ShouldDispatchGetCountForAnEntityWithNoIdentifierProperty()
		{
			//setup
			var dal = new NoIdPropertyDataAccess();
			dal.GetCountResult = 12;

			//act
			var result = dal.GetCount<Ghost>();

			//assert
			dal.GetCountWasCalled.ShouldBeTrue();
			dal.GetCountProbe.ShouldBeNull();
			result.ShouldBe(12);
		}

		[Fact]
		public void ShouldDispatchGetPagedForAnEntityWithNoIdentifierProperty()
		{
			//setup
			var dal = new NoIdPropertyDataAccess();

			//act
			var result = dal.GetPaged<Ghost>(20, 5);

			//assert
			dal.GetPagedWasCalled.ShouldBeTrue();
			dal.GetPagedProbe.ShouldBeNull();
			dal.GetPagedSkip.ShouldBe(20);
			dal.GetPagedTake.ShouldBe(5);
			result.ShouldBeSameAs(dal.GetPagedResult);
			result.Count.ShouldBe(1);
		}
	}
}
