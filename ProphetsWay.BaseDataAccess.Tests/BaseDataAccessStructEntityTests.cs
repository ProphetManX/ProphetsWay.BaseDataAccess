using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// <c>Get&lt;T&gt;(object id)</c> constrains <c>T</c> to <c>IBaseEntity, new()</c>, and a struct satisfies
	/// both. The identifier must therefore reach the derived <c>Get(T)</c> on a value-type entity exactly as it
	/// does on a reference-type one.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is the failure with no symptom. Assigning the identifier to a probe held in a variable typed
	/// <c>T</c> boxes the struct, writes to the box, and discards it; the entity handed to the derived method
	/// is a second, unmodified copy. Nothing is thrown — a real query simply runs against the default key and
	/// returns the wrong row, or no row, with no indication that anything went wrong.
	/// </para>
	/// <para>
	/// Every call below supplies the type argument explicitly. Without it the compiler binds to the derived
	/// non-generic overload and the reflection path under test is never exercised.
	/// </para>
	/// </remarks>
	public class BaseDataAccessStructEntityTests
	{
		[Fact]
		public void ShouldDispatchGetToTheDerivedMethodForAStructEntity()
		{
			//setup
			var dal = new StructEntityDataAccess();

			//act
			var result = dal.Get<Coin>(42);

			//assert
			//dispatch alone is the control for the two tests below: it isolates the identifier from the lookup
			dal.GetCoinWasCalled.ShouldBeTrue();
			result.CoinId.ShouldBe(dal.GetCoinResult.CoinId);
		}

		[Fact]
		public void ShouldAssignTheIdentifierToAStructProbeUsingTheTypeNamePrefixedProperty()
		{
			//setup
			var dal = new StructEntityDataAccess();

			//act
			dal.Get<Coin>(42);

			//assert
			dal.GetCoinWasCalled.ShouldBeTrue();
			dal.CoinProbe.CoinId.ShouldBe(42);
		}

		[Fact]
		public void ShouldAssignTheIdentifierToAStructProbeUsingTheIdFallback()
		{
			//setup
			var dal = new StructEntityDataAccess();

			//act
			dal.Get<Token>(7);

			//assert
			dal.GetTokenWasCalled.ShouldBeTrue();
			dal.TokenProbe.Id.ShouldBe(7);
		}
	}
}
