using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// The text a <see cref="DataAccessConventionException"/> renders each type as. Every type named in one of
	/// these messages is written the way it would appear in source — <c>int</c> rather than <c>Int32</c>,
	/// <c>IList&lt;Company&gt;</c> rather than the namespace-qualified backtick-arity form, <c>Company[]</c>,
	/// <c>int?</c> — and that rendering is the whole debugging experience for this exception.
	/// </summary>
	/// <remarks>
	/// <para>
	/// These are regression guards, not a specification of unwritten behaviour: the rendering already works, so
	/// every test here passes on arrival. What they buy is that a change to it fails loudly rather than quietly
	/// degrading the messages back into framework type names, which nothing else in the suite would notice.
	/// </para>
	/// <para>
	/// The renderer is private, so it is exercised through the only public route to it: the two messages that
	/// route every type they name through it — the one reporting a method that could not be found, which
	/// renders the required parameter list, and the one reporting an incompatible declared return type, which
	/// renders the declared and required types. Other files already assert the member and entity names those
	/// messages quote; these assert the rendered type text and nothing else.
	/// </para>
	/// <para>
	/// Assertions are on fragments rather than whole messages throughout. The wording has to stay free to
	/// change; the rendered types do not. Each test also denies the specific form a regression would produce —
	/// a backtick arity, a framework type name, a <c>Nullable</c> wrapper — because the absence of those is
	/// what makes pinning the presence of the right form worth anything.
	/// </para>
	/// <para>
	/// Every call supplies the type argument explicitly, for the reason given on
	/// <see cref="BaseDataAccessMethodLookupTests"/>: without it the compiler binds to the derived non-generic
	/// overload and the reflection path that builds these messages never runs.
	/// </para>
	/// </remarks>
	public class BaseDataAccessTypeRenderingTests
	{
		[Fact]
		public void ShouldRenderARequiredParameterListWithCSharpKeywords()
		{
			//setup
			var dal = new EmptyDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetPaged<Company>(0, 10));

			//assert
			ex.Message.ShouldContain("(Company, int, int)");
			ex.Message.ShouldNotContain("Int32");
			ex.Message.ShouldNotContain("`1");
		}

		[Fact]
		public void ShouldRenderASingleRequiredParameterWithoutASeparator()
		{
			//setup
			var dal = new EmptyDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetAll<Company>());

			//assert
			ex.Message.ShouldContain("(Company)");
		}

		[Fact]
		public void ShouldRenderGenericTypesAsTheyWereWrittenInSource()
		{
			//setup
			var dal = new WrongReturnTypeDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetAll<Company>());

			//assert
			ex.Message.ShouldContain("IEnumerable<Company>");
			ex.Message.ShouldContain("IList<Company>");
			ex.Message.ShouldNotContain("`1");
			ex.Message.ShouldNotContain("ProphetsWay.");
			dal.GetAllWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldRenderNestedGenericArgumentsAsTheyWereWrittenInSource()
		{
			//setup
			var dal = new NestedGenericReturnDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetAll<Company>());

			//assert
			ex.Message.ShouldContain("IDictionary<string, IList<Company>>");
			ex.Message.ShouldNotContain("`1");
			ex.Message.ShouldNotContain("`2");
			dal.GetAllWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldRenderArrayTypesWithTheirBrackets()
		{
			//setup
			var dal = new ArrayReturnCountDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetCount<Company>());

			//assert
			ex.Message.ShouldContain("[Company[]]");
			ex.Message.ShouldContain("[int]");
			ex.Message.ShouldNotContain("Int32");
			dal.GetCountWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldRenderNullableValueTypesWithAQuestionMark()
		{
			//setup
			var dal = new OptionalCountDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetCount<Company>());

			//assert
			ex.Message.ShouldContain("[int?]");
			ex.Message.ShouldNotContain("Nullable");
			ex.Message.ShouldNotContain("Int32");
			dal.GetCountWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldRenderStringAsItsKeywordRatherThanItsFrameworkName()
		{
			//setup
			var dal = new WrongReturnTypeDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetCount<Company>());

			//assert
			ShouldRenderOrdinally(ex.Message, "[string]", "[String]");
			ex.Message.ShouldContain("[int]");
			dal.GetCountWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldRenderVoidAsItsKeywordRatherThanItsFrameworkName()
		{
			//setup
			var dal = new NoResultCountDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetCount<Company>());

			//assert
			ShouldRenderOrdinally(ex.Message, "[void]", "[Void]");
			dal.GetCountWasCalled.ShouldBeFalse();
		}

		/// <summary>
		/// A keyword and the framework type name it replaces differ only by case, so these two comparisons are
		/// made ordinally rather than through Shouldly's string overloads, whose case sensitivity is a defaulted
		/// argument and would decide whether the negative assertion means anything.
		/// </summary>
		private static void ShouldRenderOrdinally(string message, string keyword, string frameworkName)
		{
			message.Contains(keyword).ShouldBeTrue($"expected the message to render '{keyword}', but it read: {message}");
			message.Contains(frameworkName).ShouldBeFalse($"expected the message not to render '{frameworkName}', but it read: {message}");
		}
	}
}
