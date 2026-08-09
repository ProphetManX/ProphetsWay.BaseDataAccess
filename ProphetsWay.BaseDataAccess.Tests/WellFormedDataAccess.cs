using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// A derived DAL that satisfies the convention for <see cref="Company"/> in every respect, and records
	/// what it was handed so dispatch and argument order can be asserted.
	/// </summary>
	public class WellFormedDataAccess : TestDataAccessBase
	{
		public bool GetAllWasCalled;
		public Company GetAllProbe;
		public IList<Company> GetAllResult = new List<Company>();

		public bool GetCountWasCalled;
		public Company GetCountProbe;
		public int GetCountResult;

		public bool GetPagedWasCalled;
		public Company GetPagedProbe;
		public int GetPagedSkip = -1;
		public int GetPagedTake = -1;
		public IList<Company> GetPagedResult = new List<Company>();

		public bool GetWasCalled;
		public Company GetProbe;
		public Company GetResult = new Company();

		public bool InsertWasCalled;
		public Company InsertItem;

		public bool UpdateWasCalled;
		public Company UpdateItem;
		public int UpdateResult;

		public bool DeleteWasCalled;
		public Company DeleteItem;
		public int DeleteResult;

		public IList<Company> GetAll(Company probe)
		{
			GetAllWasCalled = true;
			GetAllProbe = probe;
			return GetAllResult;
		}

		public int GetCount(Company probe)
		{
			GetCountWasCalled = true;
			GetCountProbe = probe;
			return GetCountResult;
		}

		public IList<Company> GetPaged(Company probe, int skip, int take)
		{
			GetPagedWasCalled = true;
			GetPagedProbe = probe;
			GetPagedSkip = skip;
			GetPagedTake = take;
			return GetPagedResult;
		}

		public Company Get(Company probe)
		{
			GetWasCalled = true;
			GetProbe = probe;
			return GetResult;
		}

		public void Insert(Company item)
		{
			InsertWasCalled = true;
			InsertItem = item;
		}

		public int Update(Company item)
		{
			UpdateWasCalled = true;
			UpdateItem = item;
			return UpdateResult;
		}

		public int Delete(Company item)
		{
			DeleteWasCalled = true;
			DeleteItem = item;
			return DeleteResult;
		}
	}
}
