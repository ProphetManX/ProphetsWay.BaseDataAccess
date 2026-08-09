using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// When a convention method is hidden with <c>new</c>, two methods with the same name and the same
	/// parameter types are visible on the concrete type. The most derived one must win, because that is the one
	/// a compile-time call on the same static type would bind to.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The two candidates are indistinguishable by name and signature, so a lookup that returns the first match
	/// it encounters is deciding by method-list enumeration order — which is unspecified and may differ between
	/// runtimes. A run that happens to pass proves nothing about the next one; only a lookup that resolves the
	/// hierarchy deliberately makes this test meaningful.
	/// </para>
	/// <para>
	/// Plain inheritance is a separate case, covered by
	/// <c>ShouldFindAConventionMethodInheritedFromAnIntermediateBaseClass</c>, and an override collapses to a
	/// single entry. Neither is affected by this.
	/// </para>
	/// </remarks>
	public class BaseDataAccessShadowedMethodTests
	{
		[Fact]
		public void ShouldBindToTheMostDerivedMethodWhenAConventionMethodIsShadowedWithNew()
		{
			//setup
			var dal = new ShadowingDataAccess();

			//act
			var result = dal.Update<Company>(new Company());

			//assert
			dal.DerivedUpdateWasCalled.ShouldBeTrue();
			dal.BaseUpdateWasCalled.ShouldBeFalse();
			result.ShouldBe(ShadowingDataAccess.DerivedUpdateResult);
		}
	}
}
