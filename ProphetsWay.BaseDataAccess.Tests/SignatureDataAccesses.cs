using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// The entity parameter is typed as the <see cref="IBaseEntity"/> interface rather than the entity type
	/// itself, so it is not an exact-signature match.
	/// </summary>
	public class InterfaceParameterDataAccess : TestDataAccessBase
	{
		public bool InsertWasCalled;

		public void Insert(IBaseEntity item)
		{
			InsertWasCalled = true;
		}
	}

	/// <summary>
	/// The entity parameter is typed as a base class of the entity rather than the entity type itself, so it
	/// is not an exact-signature match.
	/// </summary>
	public class BaseTypeParameterDataAccess : TestDataAccessBase
	{
		public bool InsertWasCalled;

		public void Insert(EntityBase item)
		{
			InsertWasCalled = true;
		}
	}

	/// <summary>
	/// <c>GetPaged</c> is missing its <c>take</c> parameter, so the arity does not match.
	/// </summary>
	public class WrongArityDataAccess : TestDataAccessBase
	{
		public bool GetPagedWasCalled;

		public IList<Company> GetPaged(Company probe, int skip)
		{
			GetPagedWasCalled = true;
			return new List<Company>();
		}
	}
}
