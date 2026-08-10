using System;
using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// The TRANSACTIONS rules stated on <see cref="IBaseDataAccess"/> and on its three transaction members,
	/// asserted against <see cref="ConformingDataAccess"/> - an implementation written to obey them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>The subject here is the implementation, not the library.</b> <see cref="BaseDataAccess"/> declares
	/// <see cref="BaseDataAccess.TransactionStart"/>, <see cref="BaseDataAccess.TransactionCommit"/> and
	/// <see cref="BaseDataAccess.TransactionRollBack"/> <c>abstract</c> and holds no transaction state, so none
	/// of this behaviour is inherited. These tests pin what an implementer must do.
	/// </para>
	/// <para>
	/// One rule from the TRANSACTIONS section is not asserted anywhere: <i>"ambient transactions are left
	/// alone"</i>. It is a statement about what these three members do <b>not</b> do to a surrounding
	/// <c>TransactionScope</c>, and its observable half belongs to the provider underneath - enrolling in the
	/// ambient scope, or not - which <see cref="ConformingDataAccess"/> has none of. A double with no provider
	/// can only demonstrate that it ignores an ambient scope, which is what it would do whether the rule existed
	/// or not, so the test would pass vacuously. This is recorded rather than left silent so the absence reads
	/// as a decision.
	/// </para>
	/// </remarks>
	public class ConformingDataAccessTransactionTests
	{
		[Fact]
		public void ShouldThrowWhenTransactionStartIsCalledWhileATransactionIsAlreadyOpen()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();

			//act
			var ex = Should.Throw<InvalidOperationException>(() => dal.TransactionStart());

			//assert
			ex.ShouldNotBeOfType<ObjectDisposedException>();
			dal.TransactionIsOpen.ShouldBeTrue();
		}

		[Fact]
		public void ShouldThrowWhenTransactionCommitIsCalledWithNoTransactionOpen()
		{
			//setup
			var dal = new ConformingDataAccess();

			//act
			var ex = Should.Throw<InvalidOperationException>(() => dal.TransactionCommit());

			//assert
			ex.ShouldNotBeOfType<ObjectDisposedException>();
		}

		[Fact]
		public void ShouldThrowWhenTransactionRollBackIsCalledWithNoTransactionOpen()
		{
			//setup
			var dal = new ConformingDataAccess();

			//act
			var ex = Should.Throw<InvalidOperationException>(() => dal.TransactionRollBack());

			//assert
			ex.ShouldNotBeOfType<ObjectDisposedException>();
		}

		[Fact]
		public void ShouldThrowWhenTransactionCommitIsCalledASecondTime()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.TransactionCommit();

			//act
			//assert
			Should.Throw<InvalidOperationException>(() => dal.TransactionCommit());
		}

		[Fact]
		public void ShouldThrowWhenTransactionRollBackIsCalledASecondTime()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.TransactionRollBack();

			//act
			//assert
			Should.Throw<InvalidOperationException>(() => dal.TransactionRollBack());
		}

		[Fact]
		public void ShouldThrowWhenTransactionRollBackFollowsACommit()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.TransactionCommit();

			//act
			//assert
			Should.Throw<InvalidOperationException>(() => dal.TransactionRollBack());
		}

		[Fact]
		public void ShouldLeaveNoTransactionOpenWhenACommitFails()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.CommitShouldFail = true;
			Should.Throw<TransactionFailureException>(() => dal.TransactionCommit());

			//act
			//assert
			dal.TransactionIsOpen.ShouldBeFalse();
			Should.Throw<InvalidOperationException>(() => dal.TransactionRollBack());
		}

		/// <summary>
		/// Anchors the two disposal tests that set <see cref="ConformingDataAccess.RollBackShouldFail"/> and can
		/// only assert that nothing was thrown: without this, a dropped or misspelled branch would leave the flag
		/// inert and those tests green while proving nothing.
		/// </summary>
		[Fact]
		public void ShouldThrowWhenTheStoreRefusesARollBack()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.RollBackShouldFail = true;

			//act
			var ex = Should.Throw<TransactionFailureException>(() => dal.TransactionRollBack());

			//assert
			ex.Message.ShouldBe(ConformingDataAccess.RollBackFailureMessage);
			dal.RollBackAttemptCount.ShouldBe(1);
			dal.TransactionIsOpen.ShouldBeFalse();
		}

		[Fact]
		public void ShouldAllowANewTransactionToBeStartedAfterACommitFails()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.CommitShouldFail = true;
			Should.Throw<TransactionFailureException>(() => dal.TransactionCommit());
			dal.CommitShouldFail = false;

			//act
			Should.NotThrow(() => dal.TransactionStart());

			//assert
			dal.TransactionIsOpen.ShouldBeTrue();
		}

		[Fact]
		public void ShouldNotPersistWritesMadeInsideATransactionThatFailedToCommit()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.Insert(new Company { Name = "Acme" });
			dal.CommitShouldFail = true;

			//act
			Should.Throw<TransactionFailureException>(() => dal.TransactionCommit());

			//assert
			dal.GetCount<Company>().ShouldBe(0);
		}

		[Fact]
		public void ShouldPersistWritesMadeInsideATransactionWhenItCommits()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.Insert(new Company { Name = "Acme" });
			dal.GetCount<Company>().ShouldBe(0);

			//act
			dal.TransactionCommit();

			//assert
			dal.GetCount<Company>().ShouldBe(1);
			dal.TransactionIsOpen.ShouldBeFalse();
		}

		[Fact]
		public void ShouldDiscardWritesMadeInsideATransactionWhenItRollsBack()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.Insert(new Company { Name = "Acme" });

			//act
			dal.TransactionRollBack();

			//assert
			dal.GetCount<Company>().ShouldBe(0);
			dal.TransactionIsOpen.ShouldBeFalse();
		}

		[Fact]
		public void ShouldAutoCommitAWriteMadeWithNoTransactionOpen()
		{
			//setup
			var dal = new ConformingDataAccess();

			//act
			dal.Insert(new Company { Name = "Acme" });

			//assert
			dal.GetCount<Company>().ShouldBe(1);
			dal.CommitAttemptCount.ShouldBe(0);
		}

		/// <summary>
		/// "Scope is the instance, not the connection" - the rule an Entity Framework backed Data Access Layer is
		/// most likely to violate, by letting two instances share one <c>DbContext</c>. Only its observable half is
		/// expressible here: two instances, one transaction, and no leakage in either direction.
		/// </summary>
		[Fact]
		public void ShouldNotEnrollAWriteMadeThroughAnotherInstanceInThisInstancesTransaction()
		{
			//setup
			var dal = new ConformingDataAccess();
			var other = new ConformingDataAccess();
			dal.TransactionStart();

			//act
			other.Insert(new Company { Name = "Acme" });
			dal.TransactionRollBack();

			//assert
			other.GetCount<Company>().ShouldBe(1);
			other.TransactionIsOpen.ShouldBeFalse();
			dal.GetCount<Company>().ShouldBe(0);
		}

		/// <summary>
		/// The other direction of the same rule: one instance closing its transaction does not close, commit or
		/// roll back a transaction open on another.
		/// </summary>
		[Fact]
		public void ShouldLeaveATransactionOpenOnAnotherInstanceUntouchedWhenThisOneCloses()
		{
			//setup
			var dal = new ConformingDataAccess();
			var other = new ConformingDataAccess();
			dal.TransactionStart();
			other.TransactionStart();
			other.Insert(new Company { Name = "Acme" });

			//act
			dal.TransactionRollBack();

			//assert
			other.TransactionIsOpen.ShouldBeTrue();
			other.RollBackAttemptCount.ShouldBe(0);
			other.TransactionCommit();
			other.GetCount<Company>().ShouldBe(1);
		}
	}
}
