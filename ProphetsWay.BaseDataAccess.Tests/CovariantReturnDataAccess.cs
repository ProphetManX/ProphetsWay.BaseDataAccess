using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Every method here satisfies the convention, but declares a return type that is <i>assignable to</i>
	/// what the matching <see cref="BaseDataAccess"/> member returns rather than identical to it —
	/// <see cref="List{T}"/> and <c>T[]</c> for <see cref="IList{T}"/>, and a derived entity type for the
	/// entity itself. These are the signatures real Data Access Layers are written with, so they must all be
	/// accepted.
	/// </summary>
	public class CovariantReturnDataAccess : TestDataAccessBase
	{
		public bool GetAllWasCalled;
		public List<Company> GetAllResult = new List<Company> { new Company { Name = "Acme" } };

		public bool GetPagedWasCalled;
		public Company[] GetPagedResult = { new Company { Name = "Acme" } };

		public bool GetWasCalled;
		public PremiumCompany GetResult = new PremiumCompany { Tier = "Gold" };

		public List<Company> GetAll(Company probe)
		{
			GetAllWasCalled = true;
			return GetAllResult;
		}

		public Company[] GetPaged(Company probe, int skip, int take)
		{
			GetPagedWasCalled = true;
			return GetPagedResult;
		}

		public PremiumCompany Get(Company probe)
		{
			GetWasCalled = true;
			return GetResult;
		}
	}
}
