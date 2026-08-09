using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Every method is located successfully by the convention and then throws from its own body.
	/// </summary>
	public class ThrowingDataAccess : TestDataAccessBase
	{
		public const string GetAllMessage = "GetAll body failed.";
		public const string GetCountMessage = "GetCount body failed.";
		public const string GetPagedMessage = "GetPaged body failed.";
		public const string GetMessage = "Get body failed.";
		public const string InsertMessage = "Insert body failed.";
		public const string UpdateMessage = "Update body failed.";
		public const string DeleteMessage = "Delete body failed.";

		public IList<Company> GetAll(Company probe)
		{
			throw new DerivedMethodException(GetAllMessage);
		}

		public int GetCount(Company probe)
		{
			throw new DerivedMethodException(GetCountMessage);
		}

		public IList<Company> GetPaged(Company probe, int skip, int take)
		{
			throw new DerivedMethodException(GetPagedMessage);
		}

		public Company Get(Company probe)
		{
			throw new DerivedMethodException(GetMessage);
		}

		public void Insert(Company item)
		{
			throw new DerivedMethodException(InsertMessage);
		}

		public int Update(Company item)
		{
			throw new DerivedMethodException(UpdateMessage);
		}

		public int Delete(Company item)
		{
			throw new DerivedMethodException(DeleteMessage);
		}
	}

	/// <summary>
	/// Satisfies the convention for <see cref="Detonator"/>, whose constructor throws. The body is never
	/// reached: <c>Get&lt;T&gt;</c> fails while constructing the probe entity.
	/// </summary>
	public class DetonatorDataAccess : TestDataAccessBase
	{
		public bool GetWasCalled;

		public Detonator Get(Detonator probe)
		{
			GetWasCalled = true;
			return probe;
		}
	}
}
