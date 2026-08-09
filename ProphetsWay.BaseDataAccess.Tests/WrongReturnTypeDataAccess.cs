using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Every method here is found by the convention lookup, but <i>declares</i> a return type that cannot be
	/// assigned to the return type the matching <see cref="BaseDataAccess"/> member declares.
	/// </summary>
	/// <remarks>
	/// The invocation probes record whether each body actually ran, and the answer is the same for all six:
	/// never. The return-type check is made against the declared type before the method is invoked, for reads
	/// and writes alike, so no body below should ever execute. The values they return exist only to make them
	/// compile — nothing reads them.
	/// </remarks>
	public class WrongReturnTypeDataAccess : TestDataAccessBase
	{
		public bool GetAllWasCalled;
		public bool GetPagedWasCalled;
		public bool GetCountWasCalled;
		public bool GetWasCalled;
		public bool UpdateWasCalled;
		public bool DeleteWasCalled;

		public IEnumerable<Company> GetAll(Company probe)
		{
			GetAllWasCalled = true;
			return new HashSet<Company>();
		}

		public IEnumerable<Company> GetPaged(Company probe, int skip, int take)
		{
			GetPagedWasCalled = true;
			return new HashSet<Company>();
		}

		public string GetCount(Company probe)
		{
			GetCountWasCalled = true;
			return "not an int";
		}

		public object Get(Company probe)
		{
			GetWasCalled = true;
			return "not a Company";
		}

		public string Update(Company item)
		{
			UpdateWasCalled = true;
			return "not an int";
		}

		public string Delete(Company item)
		{
			DeleteWasCalled = true;
			return "not an int";
		}
	}
}
