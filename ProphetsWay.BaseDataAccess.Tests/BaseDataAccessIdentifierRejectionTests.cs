using System;
using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// An identifier <c>Get&lt;T&gt;(object id)</c> cannot write to the resolved identifier property is the
	/// caller's mistake, not a wiring error in the entity, and the library says so by letting the
	/// <see cref="ArgumentException"/> stand rather than reinterpreting it as a
	/// <see cref="DataAccessConventionException"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// That ruling is stated on the identifier assignment step itself, and it covers <c>null</c> exactly as it
	/// covers a wrong type: a non-nullable value-type identifier property cannot hold <c>null</c> either. The
	/// reflection layer is simply lenient about it, converting <c>null</c> to <c>default</c> and letting a
	/// probe for identifier zero go out as though the caller had asked for it — a caller bug returned as a
	/// plausible "no such row". Whether the rejection happens is therefore this library's to promise, not the
	/// CLR's, and it is promised here.
	/// </para>
	/// <para>
	/// The exception type is asserted in both directions on purpose. <see cref="ArgumentException"/> and
	/// <see cref="DataAccessConventionException"/> are the two halves of a deliberate split — caller error
	/// against wiring error — and they are unrelated by inheritance, so the distinction is one an assertion can
	/// genuinely hold onto rather than a distinction only the reader can see.
	/// </para>
	/// <para>
	/// Rejection is asserted to happen before the derived <c>Get</c> runs, matching every other pre-dispatch
	/// check in the convention: a call that cannot be formed correctly must not reach the database first and
	/// report the defect afterwards.
	/// </para>
	/// <para>
	/// Every call below supplies the type argument explicitly. Without it the compiler binds to the derived
	/// non-generic overload and the reflection path under test is never exercised.
	/// </para>
	/// </remarks>
	public class BaseDataAccessIdentifierRejectionTests
	{
		[Fact]
		public void ShouldRejectANullIdentifierWhenTheIdentifierPropertyIsANonNullableValueType()
		{
			//setup
			var dal = new WellFormedDataAccess();

			//act
			var ex = Should.Throw<ArgumentException>(() => dal.Get<Company>(null));

			//assert
			ex.ShouldNotBeAssignableTo<DataAccessConventionException>();
			dal.GetWasCalled.ShouldBeFalse();
		}

		[Theory]
		[InlineData("42")]
		[InlineData(42L)]
		public void ShouldRejectAnIdentifierOfATypeTheIdentifierPropertyCannotHold(object id)
		{
			//setup
			//a string shares nothing with int, and a long is a value type the reflection binder still refuses to
			//narrow, so the two rows are wrong in genuinely different ways rather than twice over in one way
			var dal = new WellFormedDataAccess();

			//act
			var ex = Should.Throw<ArgumentException>(() => dal.Get<Company>(id));

			//assert
			ex.ShouldNotBeAssignableTo<DataAccessConventionException>();
			dal.GetWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldAcceptAnIdentifierOfTheTypeTheIdentifierPropertyHolds()
		{
			//setup
			//without this the rejections above are all satisfied by a guard that refuses every identifier
			var dal = new WellFormedDataAccess();

			//act
			var result = dal.Get<Company>(42);

			//assert
			dal.GetWasCalled.ShouldBeTrue();
			dal.GetProbe.CompanyId.ShouldBe(42);
			result.ShouldBeSameAs(dal.GetResult);
		}
	}
}
