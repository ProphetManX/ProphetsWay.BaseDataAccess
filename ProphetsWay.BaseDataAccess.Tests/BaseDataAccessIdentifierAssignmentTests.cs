using System;
using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Writing the identifier onto the probe entity is the last step in <c>Get&lt;T&gt;(object id)</c> that can
	/// still fail, and each way it can fail belongs to one of the two error contracts the library already
	/// honours everywhere else.
	/// </summary>
	/// <remarks>
	/// <para>
	/// An identifier property that cannot be written to is a wiring error in the entity, indistinguishable in
	/// kind from an entity that exposes no identifier property at all, and is reported the same way — a
	/// <see cref="DataAccessConventionException"/> naming the type and the property, thrown before the derived
	/// method runs. "Cannot be written to" means no set accessor at all; a non-public one is resolved and
	/// invoked by reflection and is not a failure.
	/// </para>
	/// <para>
	/// An identifier setter that exists and throws is not a wiring error. It is the entity's own code failing,
	/// and it reaches the caller as itself, exactly as an exception from a derived method body or from an
	/// entity constructor already does — never as a <c>TargetInvocationException</c>.
	/// </para>
	/// <para>
	/// An identifier of the wrong type is neither: it is the caller's mistake. The reflection layer rejects it
	/// with an <see cref="ArgumentException"/> and the library lets that stand rather than reinterpreting it,
	/// which is the same ruling the library makes for itself when a null identifier is written to a non-nullable
	/// value-type property — a case reflection would let through, so the library rejects it the same way before
	/// reflection is reached.
	/// </para>
	/// <para>
	/// Every call below supplies the type argument explicitly. Without it the compiler binds to the derived
	/// non-generic overload and the reflection path under test is never exercised.
	/// </para>
	/// </remarks>
	public class BaseDataAccessIdentifierAssignmentTests
	{
		[Fact]
		public void ShouldThrowConventionExceptionWhenTheIdentifierPropertyHasNoSetter()
		{
			//setup
			var dal = new IdentifierAssignmentDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Get<Sprocket>(5));

			//assert
			//the property is named single-quoted for the same reason the missing-property message is: an
			//unquoted assertion is satisfied by a message that merely mentions the type
			ex.Message.ShouldContain(nameof(Sprocket));
			ex.Message.ShouldContain("'SprocketId'");
			dal.GetSprocketWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldPropagateTheOriginalExceptionWhenTheIdentifierSetterThrows()
		{
			//setup
			var dal = new IdentifierAssignmentDataAccess();

			//act
			var ex = Should.Throw<IdentifierAssignmentException>(() => dal.Get<Landmine>(9));

			//assert
			ex.Message.ShouldBe(Landmine.SetterMessage);
			dal.GetLandmineWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldPreserveTheStackWhenTheIdentifierSetterThrows()
		{
			//setup
			var dal = new IdentifierAssignmentDataAccess();

			//act
			var ex = Should.Throw<IdentifierAssignmentException>(() => dal.Get<Landmine>(9));

			//assert
			//rethrowing the inner exception directly would satisfy the test above while discarding the frame
			//that names the setter, which is the only thing telling the reader where the failure came from
			ex.StackTrace.ShouldNotBeNullOrEmpty();
			ex.StackTrace.ShouldContain(nameof(Landmine));
		}

		[Fact]
		public void ShouldNotReportAConventionFailureWhenTheIdentifierIsOfTheWrongType()
		{
			//setup
			var dal = new WellFormedDataAccess();

			//act
			var ex = Should.Throw<ArgumentException>(() => dal.Get<Company>("not-an-integer"));

			//assert
			//the two error contracts are unrelated by inheritance, so this holds onto the caller-error against
			//wiring-error split rather than leaving it to the reader
			ex.ShouldNotBeAssignableTo<DataAccessConventionException>();
			dal.GetWasCalled.ShouldBeFalse();
		}
	}
}
