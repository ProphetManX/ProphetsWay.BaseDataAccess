using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// The required method exists with the correct signature, but is <c>private</c>.
	/// </summary>
	public class PrivateMethodDataAccess : TestDataAccessBase
	{
		private IList<Company> GetAll(Company probe)
		{
			return new List<Company>();
		}
	}

	/// <summary>
	/// The required method exists with the correct signature, but is <c>protected</c>.
	/// </summary>
	public class ProtectedMethodDataAccess : TestDataAccessBase
	{
		protected IList<Company> GetAll(Company probe)
		{
			return new List<Company>();
		}
	}

	/// <summary>
	/// The required method exists with the correct signature, but is <c>internal</c>.
	/// </summary>
	public class InternalMethodDataAccess : TestDataAccessBase
	{
		internal IList<Company> GetAll(Company probe)
		{
			return new List<Company>();
		}
	}

	/// <summary>
	/// The required method exists with the correct signature and is public, but is <c>static</c>.
	/// </summary>
	public class StaticMethodDataAccess : TestDataAccessBase
	{
		public static IList<Company> GetAll(Company probe)
		{
			return new List<Company>();
		}
	}

	/// <summary>
	/// A second member declared <c>private</c>, so the visibility rule is proved on more than one convention
	/// method and a per-member divergence in binding flags cannot pass unnoticed.
	/// </summary>
	public class PrivateGetPagedDataAccess : TestDataAccessBase
	{
		public bool GetPagedWasCalled;

		private IList<Company> GetPaged(Company probe, int skip, int take)
		{
			GetPagedWasCalled = true;
			return new List<Company>();
		}
	}
}
