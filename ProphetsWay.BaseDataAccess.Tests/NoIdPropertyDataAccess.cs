using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// A well-formed DAL for <see cref="Ghost"/>, an entity exposing neither <c>GhostId</c> nor <c>Id</c>,
	/// covering only the three members that pass <c>null</c> as the entity argument.
	/// </summary>
	/// <remarks>
	/// <c>GetAll</c>, <c>GetCount</c> and <c>GetPaged</c> never construct a probe entity, so they never look
	/// for an identifier property and must work against an entity that has none. Only <c>Get&lt;T&gt;(object)</c>
	/// requires one. Deliberately no <c>Get(Ghost)</c> here — that case belongs to
	/// <see cref="IdResolutionDataAccess"/>.
	/// </remarks>
	public class NoIdPropertyDataAccess : TestDataAccessBase
	{
		public bool GetAllWasCalled;
		public Ghost GetAllProbe;
		public IList<Ghost> GetAllResult = new List<Ghost> { new Ghost { Label = "Casper" } };

		public bool GetCountWasCalled;
		public Ghost GetCountProbe;
		public int GetCountResult = 7;

		public bool GetPagedWasCalled;
		public Ghost GetPagedProbe;
		public int GetPagedSkip = -1;
		public int GetPagedTake = -1;
		public IList<Ghost> GetPagedResult = new List<Ghost> { new Ghost { Label = "Casper" } };

		public IList<Ghost> GetAll(Ghost probe)
		{
			GetAllWasCalled = true;
			GetAllProbe = probe;
			return GetAllResult;
		}

		public int GetCount(Ghost probe)
		{
			GetCountWasCalled = true;
			GetCountProbe = probe;
			return GetCountResult;
		}

		public IList<Ghost> GetPaged(Ghost probe, int skip, int take)
		{
			GetPagedWasCalled = true;
			GetPagedProbe = probe;
			GetPagedSkip = skip;
			GetPagedTake = take;
			return GetPagedResult;
		}
	}
}
