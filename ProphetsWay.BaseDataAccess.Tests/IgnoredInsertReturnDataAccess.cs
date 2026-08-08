namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// An <c>Insert</c> declared to return <see cref="string"/>, which the convention permits.
	/// </summary>
	/// <remarks>
	/// <c>Insert&lt;T&gt;</c> discards whatever the derived method produced, so its declared return type is
	/// unconstrained — <c>void</c>, <c>int</c>, <c>string</c> and anything else are all legal. This double
	/// exists so that adding a return guard to <c>Insert</c> fails a test instead of passing unnoticed.
	/// </remarks>
	public class IgnoredInsertReturnDataAccess : TestDataAccessBase
	{
		public bool InsertWasCalled;
		public Company InsertItem;

		public string Insert(Company item)
		{
			InsertWasCalled = true;
			InsertItem = item;
			return "a value nobody reads";
		}
	}
}
