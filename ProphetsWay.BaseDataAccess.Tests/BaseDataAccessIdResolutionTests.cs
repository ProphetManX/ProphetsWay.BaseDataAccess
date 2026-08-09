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
	}
}
