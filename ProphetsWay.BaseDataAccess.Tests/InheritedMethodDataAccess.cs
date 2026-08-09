using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// An intermediate layer that declares the convention method once so several concrete Data Access Layers
	/// can share it. This is the ordinary way a real DAL factors out common CRUD.
	/// </summary>
	public abstract class SharedConventionBase : TestDataAccessBase
	{
		public bool GetAllWasCalled;
		public IList<Company> GetAllResult = new List<Company> { new Company { Name = "Acme" } };

		public IList<Company> GetAll(Company probe)
		{
			GetAllWasCalled = true;
			return GetAllResult;
		}
	}

	/// <summary>
	/// Declares no convention methods of its own; the required <c>GetAll(Company)</c> is inherited from
	/// <see cref="SharedConventionBase"/> and must still be found.
	/// </summary>
	public class InheritedMethodDataAccess : SharedConventionBase
	{
	}
}
