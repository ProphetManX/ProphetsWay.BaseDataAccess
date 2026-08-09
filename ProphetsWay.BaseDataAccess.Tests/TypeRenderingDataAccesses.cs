using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Declares an array where the convention requires an <see cref="int"/>, so the array is the <i>wrong</i>
	/// declared return type and the message that reports it has to render one.
	/// </summary>
	/// <remarks>
	/// <see cref="CovariantReturnDataAccess"/> returns an array successfully and therefore produces no message
	/// at all, so it cannot pin how an array is rendered. This double exists only to force that rendering onto
	/// a message, and is deliberately separate so the existing double's shape stays untouched.
	/// </remarks>
	public class ArrayReturnCountDataAccess : TestDataAccessBase
	{
		public bool GetCountWasCalled;

		public Company[] GetCount(Company probe)
		{
			GetCountWasCalled = true;
			return new Company[0];
		}
	}

	/// <summary>
	/// Declares a nullable value type where the convention requires a non-nullable <see cref="int"/>, which the
	/// return-type gate rejects — so the message has to render the nullable form.
	/// </summary>
	/// <remarks>
	/// Named without the words its test denies. Every message quotes the offending data access type by name, so
	/// a double called <c>Nullable…</c> would satisfy that test's <c>ShouldNotContain("Nullable")</c> by itself.
	/// </remarks>
	public class OptionalCountDataAccess : TestDataAccessBase
	{
		public bool GetCountWasCalled;

		public int? GetCount(Company probe)
		{
			GetCountWasCalled = true;
			return null;
		}
	}

	/// <summary>
	/// Declares <c>void</c> where the convention requires an <see cref="int"/>, so <c>void</c> — a keyword with
	/// no other route onto a message, since <c>Insert</c> leaves its return type unconstrained — is rendered.
	/// </summary>
	/// <remarks>
	/// Named without the word its test denies, for the reason given on <see cref="OptionalCountDataAccess"/>.
	/// </remarks>
	public class NoResultCountDataAccess : TestDataAccessBase
	{
		public bool GetCountWasCalled;

		public void GetCount(Company probe)
		{
			GetCountWasCalled = true;
		}
	}

	/// <summary>
	/// Declares a generic type whose arguments are themselves a keyword and a generic type, so the message has
	/// to render the nesting, the argument separator, and a keyword substitution inside an argument position.
	/// </summary>
	public class NestedGenericReturnDataAccess : TestDataAccessBase
	{
		public bool GetAllWasCalled;

		public IDictionary<string, IList<Company>> GetAll(Company probe)
		{
			GetAllWasCalled = true;
			return new Dictionary<string, IList<Company>>();
		}
	}
}
