namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// An intermediate layer declaring the convention method, which a concrete Data Access Layer then replaces
	/// outright with <c>new</c> rather than overriding it.
	/// </summary>
	/// <remarks>
	/// This differs from <see cref="SharedConventionBase"/> in the one way that matters to the lookup: an
	/// override collapses to a single most-derived entry, whereas a <c>new</c>-shadowed method leaves two
	/// entries with identical names and identical parameter types on the same type's method list.
	/// </remarks>
	public abstract class ShadowedConventionBase : TestDataAccessBase
	{
		public const int BaseUpdateResult = 111;

		public bool BaseUpdateWasCalled;

		public int Update(Company item)
		{
			BaseUpdateWasCalled = true;
			return BaseUpdateResult;
		}
	}

	/// <summary>
	/// Hides the inherited <c>Update(Company)</c> with its own. C# binds a compile-time call on this static
	/// type to this method, and the convention must resolve to the same one.
	/// </summary>
	public class ShadowingDataAccess : ShadowedConventionBase
	{
		public const int DerivedUpdateResult = 222;

		public bool DerivedUpdateWasCalled;

		public new int Update(Company item)
		{
			DerivedUpdateWasCalled = true;
			return DerivedUpdateResult;
		}
	}
}
