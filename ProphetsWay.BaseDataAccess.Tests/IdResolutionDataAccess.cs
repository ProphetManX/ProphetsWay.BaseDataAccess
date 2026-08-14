namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Records the probe entity that <c>Get&lt;T&gt;</c> constructed, so the resolved identifier property and
	/// the value written to it can be asserted.
	/// </summary>
	public class IdResolutionDataAccess : TestDataAccessBase
	{
		public Company ReceivedCompany;
		public Widget ReceivedWidget;
		public Gadget ReceivedGadget;
		public Ghost ReceivedGhost;
		public Wraith ReceivedWraith;

		public Company Get(Company probe)
		{
			ReceivedCompany = probe;
			return probe;
		}

		public Widget Get(Widget probe)
		{
			ReceivedWidget = probe;
			return probe;
		}

		public Gadget Get(Gadget probe)
		{
			ReceivedGadget = probe;
			return probe;
		}

		public Ghost Get(Ghost probe)
		{
			ReceivedGhost = probe;
			return probe;
		}

		//present so the method lookup succeeds and the failure under test can only be identifier resolution
		public Wraith Get(Wraith probe)
		{
			ReceivedWraith = probe;
			return probe;
		}
	}
}
