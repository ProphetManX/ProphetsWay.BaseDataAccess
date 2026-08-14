using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// How <c>Get&lt;T&gt;(object id)</c> chooses the identifier property on the probe entity it constructs.
	/// </summary>
	/// <remarks>
	/// The failure message must name the property single-quoted. Unquoted, <c>ShouldContain("Id")</c> is
	/// satisfied by any message that merely happens to mention a property such as <c>CompanyId</c>, which makes
	/// the assertion worthless.
	/// </remarks>
	public class BaseDataAccessIdResolutionTests
	{
		[Fact]
		public void ShouldUseTheTypeNamePrefixedIdPropertyWhenItIsTheOnlyOne()
		{
			//setup
			var dal = new IdResolutionDataAccess();

			//act
			dal.Get<Company>(42);

			//assert
			dal.ReceivedCompany.ShouldNotBeNull();
			dal.ReceivedCompany.CompanyId.ShouldBe(42);
		}

		[Fact]
		public void ShouldFallBackToTheIdPropertyWhenThereIsNoTypeNamePrefixedOne()
		{
			//setup
			var dal = new IdResolutionDataAccess();

			//act
			dal.Get<Widget>(7);

			//assert
			dal.ReceivedWidget.ShouldNotBeNull();
			dal.ReceivedWidget.Id.ShouldBe(7);
		}

		[Fact]
		public void ShouldPreferTheTypeNamePrefixedIdPropertyWhenBothExist()
		{
			//setup
			var dal = new IdResolutionDataAccess();

			//act
			dal.Get<Gadget>(99);

			//assert
			dal.ReceivedGadget.ShouldNotBeNull();
			dal.ReceivedGadget.GadgetId.ShouldBe(99);
			dal.ReceivedGadget.Id.ShouldBe(0);
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenTheEntityHasNoIdProperty()
		{
			//setup
			var dal = new IdResolutionDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Get<Ghost>(1));

			//assert
			ex.Message.ShouldContain(nameof(Ghost));
			ex.Message.ShouldContain("'Id'");
			dal.ReceivedGhost.ShouldBeNull();
		}

		/// <summary>
		/// Pins what an <b>explicitly</b> implemented <see cref="IBaseIdEntity{T}"/> identifier does. The entity
		/// carries an identifier the compiler has verified, yet resolution is by name against the public surface
		/// and an explicit implementation is private under an interface-prefixed name, so nothing matches.
		/// </summary>
		/// <remarks>
		/// The message is asserted to name the two properties it looked for rather than merely to be of the right
		/// type, because the whole risk of this case is that the reported defect - "no identifier property" - and
		/// the actual defect - "the identifier is not public under either name" - read as contradicting each other
		/// to a developer looking at an entity that plainly declares one.
		/// </remarks>
		[Fact]
		public void ShouldThrowConventionExceptionWhenTheEntityImplementsItsIdentifierExplicitly()
		{
			//setup
			var dal = new IdResolutionDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Get<Wraith>(5));

			//assert
			ex.Message.ShouldContain(nameof(Wraith));
			ex.Message.ShouldContain("'WraithId'");
			ex.Message.ShouldContain("'Id'");
			dal.ReceivedWraith.ShouldBeNull();
		}
	}
}
