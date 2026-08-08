namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Satisfies the convention for the entities whose identifier property can legitimately hold <c>null</c>,
	/// and records the probe entity it was handed so a null identifier can be told apart from a method that was
	/// never reached.
	/// </summary>
	public class NullIdDataAccess : TestDataAccessBase
	{
		public bool GetTicketWasCalled;
		public Ticket TicketProbe;
		public Ticket GetTicketResult = new Ticket();

		public bool GetCouponWasCalled;
		public Coupon CouponProbe;
		public Coupon GetCouponResult = new Coupon();

		public Ticket Get(Ticket probe)
		{
			GetTicketWasCalled = true;
			TicketProbe = probe;
			return GetTicketResult;
		}

		public Coupon Get(Coupon probe)
		{
			GetCouponWasCalled = true;
			CouponProbe = probe;
			return GetCouponResult;
		}
	}
}
