using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Locating the derived method: absence, insufficient visibility, and signatures that do not match exactly
	/// are all the same failure and must all report it the same way.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every call below supplies the type argument explicitly. Without it the compiler binds to the derived
	/// non-generic overload and the reflection path under test is never exercised.
	/// </para>
	/// <para>
	/// The message assertions require the offending member name to appear <i>single-quoted</i>, and assert the
	/// absence of its siblings. That is deliberate: several convention members share a prefix, so an unquoted
	/// <c>ShouldContain("Get")</c> is satisfied by a message that only ever mentions <c>GetAll</c>, and one
	/// hardcoded message naming all seven members would satisfy every positive assertion in this file. The
	/// quoting is part of the contract, not incidental formatting.
	/// </para>
	/// </remarks>
	public class BaseDataAccessMethodLookupTests
	{
		[Fact]
		public void ShouldThrowConventionExceptionWhenGetAllIsMissing()
		{
			//setup
			var dal = new EmptyDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetAll<Company>());

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetAll'");
			ex.Message.ShouldNotContain("'Get'");
			ex.Message.ShouldNotContain("'GetPaged'");
			ex.Message.ShouldNotContain("'GetCount'");
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenGetCountIsMissing()
		{
			//setup
			var dal = new EmptyDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetCount<Company>());

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetCount'");
			ex.Message.ShouldNotContain("'Get'");
			ex.Message.ShouldNotContain("'GetAll'");
			ex.Message.ShouldNotContain("'GetPaged'");
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenGetPagedIsMissing()
		{
			//setup
			var dal = new EmptyDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetPaged<Company>(0, 10));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetPaged'");
			ex.Message.ShouldNotContain("'Get'");
			ex.Message.ShouldNotContain("'GetAll'");
			ex.Message.ShouldNotContain("'GetCount'");
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenGetIsMissing()
		{
			//setup
			var dal = new EmptyDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Get<Company>(1));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'Get'");
			ex.Message.ShouldNotContain("GetAll");
			ex.Message.ShouldNotContain("GetPaged");
			ex.Message.ShouldNotContain("GetCount");
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenInsertIsMissing()
		{
			//setup
			var dal = new EmptyDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Insert<Company>(new Company()));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'Insert'");
			ex.Message.ShouldNotContain("'Update'");
			ex.Message.ShouldNotContain("'Delete'");
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenUpdateIsMissing()
		{
			//setup
			var dal = new EmptyDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Update<Company>(new Company()));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'Update'");
			ex.Message.ShouldNotContain("'Insert'");
			ex.Message.ShouldNotContain("'Delete'");
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenDeleteIsMissing()
		{
			//setup
			var dal = new EmptyDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Delete<Company>(new Company()));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'Delete'");
			ex.Message.ShouldNotContain("'Insert'");
			ex.Message.ShouldNotContain("'Update'");
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenTheDerivedMethodIsPrivate()
		{
			//setup
			var dal = new PrivateMethodDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetAll<Company>());

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetAll'");
			ex.Message.ShouldNotContain("'Get'");
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenTheDerivedMethodIsProtected()
		{
			//setup
			var dal = new ProtectedMethodDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetAll<Company>());

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetAll'");
			ex.Message.ShouldNotContain("'Get'");
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenTheDerivedMethodIsInternal()
		{
			//setup
			var dal = new InternalMethodDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetAll<Company>());

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetAll'");
			ex.Message.ShouldNotContain("'Get'");
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenTheDerivedMethodIsStatic()
		{
			//setup
			var dal = new StaticMethodDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetAll<Company>());

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetAll'");
			ex.Message.ShouldNotContain("'Get'");
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenGetPagedIsPrivate()
		{
			//setup
			var dal = new PrivateGetPagedDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetPaged<Company>(0, 10));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetPaged'");
			ex.Message.ShouldNotContain("'Get'");
			dal.GetPagedWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenTheEntityParameterIsTypedAsAnInterface()
		{
			//setup
			var dal = new InterfaceParameterDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Insert<Company>(new Company()));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'Insert'");
			ex.Message.ShouldNotContain("'Update'");
			ex.Message.ShouldNotContain("'Delete'");
			dal.InsertWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenTheEntityParameterIsTypedAsABaseClass()
		{
			//setup
			var dal = new BaseTypeParameterDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Insert<Product>(new Product()));

			//assert
			ex.Message.ShouldContain(nameof(Product));
			ex.Message.ShouldContain("'Insert'");
			ex.Message.ShouldNotContain("'Update'");
			ex.Message.ShouldNotContain("'Delete'");
			dal.InsertWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldThrowConventionExceptionWhenTheDerivedMethodHasTheWrongArity()
		{
			//setup
			var dal = new WrongArityDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.GetPaged<Company>(0, 10));

			//assert
			ex.Message.ShouldContain(nameof(Company));
			ex.Message.ShouldContain("'GetPaged'");
			ex.Message.ShouldNotContain("'Get'");
			dal.GetPagedWasCalled.ShouldBeFalse();
		}

		[Fact]
		public void ShouldFindAConventionMethodInheritedFromAnIntermediateBaseClass()
		{
			//setup
			var dal = new InheritedMethodDataAccess();

			//act
			var result = dal.GetAll<Company>();

			//assert
			dal.GetAllWasCalled.ShouldBeTrue();
			result.ShouldBeSameAs(dal.GetAllResult);
			result.Count.ShouldBe(1);
		}

		[Fact]
		public void ShouldReportTheMissingMethodWhenTheEntityConstructorAlsoThrows()
		{
			//setup
			var dal = new EmptyDataAccess();

			//act
			var ex = Should.Throw<DataAccessConventionException>(() => dal.Get<Detonator>(1));

			//assert
			//the constructor throws unconditionally, so a DataAccessConventionException reaching the caller is
			//itself the proof that the lookup ran first and no probe entity was ever built
			ex.Message.ShouldContain(nameof(Detonator));
			ex.Message.ShouldContain("'Get'");
			(ex.InnerException as EntityConstructorException).ShouldBeNull();
		}
	}
}
