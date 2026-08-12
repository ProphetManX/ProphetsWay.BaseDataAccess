using System;
using Shouldly;
using Xunit;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// The DISPOSAL rules stated on <see cref="IBaseDataAccess"/>, asserted against
	/// <see cref="ConformingDataAccess"/> - an implementation written to obey them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>The subject here is the implementation, not the library.</b> <see cref="BaseDataAccess"/> declares
	/// <c>Dispose</c> and the transaction members <c>abstract</c> and owns no connection, context or transaction
	/// state, so it neither performs nor enforces any of this. These tests pin what an implementer must do; they
	/// deliberately do not claim that inheriting <see cref="BaseDataAccess"/> gets it done for you.
	/// </para>
	/// <para>
	/// One rule from the DISPOSAL section is <b>not expressible here</b>: <i>"the Data Access Layer disposes
	/// what it created; anything handed to it belongs to the caller"</i>. <see cref="IBaseDataAccess"/> exposes
	/// no constructor or factory surface, so which resources were passed in and which were created is not
	/// observable through the contract, and a test written against
	/// <see cref="ConformingDataAccess"/> could only exercise a constructor invented for that test. The rule is
	/// testable in a real Data Access Layer that takes an injected context or connection - by asserting the
	/// injected resource is still usable after the Data Access Layer is disposed - so it belongs to a future
	/// conformance suite run against real implementations, not to the "untestable" pile.
	/// </para>
	/// <para>
	/// Several assertions below read <see cref="ConformingDataAccess.Committed"/> or
	/// <see cref="ConformingDataAccess.Pending"/> rather than going through <c>GetCount</c>. That is forced:
	/// every member but <c>Dispose</c> throws once the instance is disposed, so what survived disposal cannot be
	/// read through the contract at the only moment worth reading it.
	/// </para>
	/// </remarks>
	public class ConformingDataAccessDisposalTests
	{
		[Fact]
		public void ShouldNotThrowWhenDisposeIsCalledRepeatedly()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.Dispose();

			//act
			Should.NotThrow(() => dal.Dispose());
			Should.NotThrow(() => dal.Dispose());

			//assert
			dal.DisposeCallCount.ShouldBe(3);
			dal.IsDisposed.ShouldBeTrue();
		}

		[Fact]
		public void ShouldTreatEveryDisposeAfterTheFirstAsANoOp()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.Insert(new Company { Name = "Acme" });

			//act
			dal.Dispose();
			dal.Dispose();
			dal.Dispose();

			//assert
			dal.RollBackAttemptCount.ShouldBe(1);
			dal.DisposeCallCount.ShouldBe(3);
		}

		[Fact]
		public void ShouldRollBackRatherThanCommitATransactionLeftOpenWhenDisposed()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.Insert(new Company { Name = "Acme" });
			dal.Pending.Count.ShouldBe(1);

			//act
			dal.Dispose();

			//assert
			dal.RollBackAttemptCount.ShouldBe(1);
			dal.CommitAttemptCount.ShouldBe(0);
			dal.Committed.ShouldBeEmpty();
			dal.Pending.ShouldBeEmpty();
			dal.TransactionIsOpen.ShouldBeFalse();
		}

		[Fact]
		public void ShouldLeaveAlreadyCommittedWorkPersistedWhenDisposed()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.Insert(new Company { Name = "Acme" });
			dal.TransactionCommit();

			//act
			dal.Dispose();

			//assert
			dal.Committed.Count.ShouldBe(1);
			dal.RollBackAttemptCount.ShouldBe(0);
		}

		[Fact]
		public void ShouldNotAttemptARollBackWhenDisposedWithNoTransactionOpen()
		{
			//setup
			var dal = new ConformingDataAccess();

			//act
			Should.NotThrow(() => dal.Dispose());

			//assert
			dal.RollBackAttemptCount.ShouldBe(0);
			dal.CommitAttemptCount.ShouldBe(0);
			dal.IsDisposed.ShouldBeTrue();
		}

		/// <summary>
		/// That the flag this test sets is genuinely read - rather than inert, leaving "nothing was thrown"
		/// trivially true - is proved by
		/// <see cref="ConformingDataAccessTransactionTests.ShouldThrowWhenTheStoreRefusesARollBack"/>.
		/// </summary>
		[Fact]
		public void ShouldNotThrowWhenTheRollBackPerformedDuringDisposalFails()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.Insert(new Company { Name = "Acme" });
			dal.RollBackShouldFail = true;

			//act
			Should.NotThrow(() => dal.Dispose());

			//assert
			dal.RollBackAttemptCount.ShouldBe(1);
			dal.Committed.ShouldBeEmpty();
		}

		/// <summary>
		/// Companion to <see cref="ShouldNotThrowWhenTheRollBackPerformedDuringDisposalFails"/>; the flag it sets
		/// is proved live by
		/// <see cref="ConformingDataAccessTransactionTests.ShouldThrowWhenTheStoreRefusesARollBack"/>.
		/// </summary>
		[Fact]
		public void ShouldStillBeDisposedWhenTheRollBackPerformedDuringDisposalFails()
		{
			//setup
			var dal = new ConformingDataAccess();
			dal.TransactionStart();
			dal.RollBackShouldFail = true;

			//act
			dal.Dispose();

			//assert
			dal.IsDisposed.ShouldBeTrue();
			Should.Throw<ObjectDisposedException>(() => dal.GetCount<Company>());
		}

		[Fact]
		public void ShouldNotThrowWhenAnyMemberIsCalledBeforeDisposal()
		{
			//setup
			var dal = new ConformingDataAccess();
			var item = new Company { Name = "Acme" };

			//act
			//assert
			Should.NotThrow(() => dal.GetAll<Company>());
			Should.NotThrow(() => dal.GetPaged<Company>(0, 10));
			Should.NotThrow(() => dal.GetCount<Company>());
			Should.NotThrow(() => dal.Get<Company>(1));
			Should.NotThrow(() => dal.Insert(item));
			Should.NotThrow(() => dal.Update(item));
			Should.NotThrow(() => dal.Delete(item));
			Should.NotThrow(() => dal.TransactionStart());
			Should.NotThrow(() => dal.TransactionCommit());
			Should.NotThrow(() => dal.TransactionStart());
			Should.NotThrow(() => dal.TransactionRollBack());
		}

		[Fact]
		public void ShouldThrowObjectDisposedExceptionWhenGetAllIsCalledAfterDisposal()
		{
			//setup
			var dal = Disposed();

			//act
			//assert
			Should.Throw<ObjectDisposedException>(() => dal.GetAll<Company>());
		}

		[Fact]
		public void ShouldThrowObjectDisposedExceptionWhenGetPagedIsCalledAfterDisposal()
		{
			//setup
			var dal = Disposed();

			//act
			//assert
			Should.Throw<ObjectDisposedException>(() => dal.GetPaged<Company>(0, 10));
		}

		[Fact]
		public void ShouldThrowObjectDisposedExceptionWhenGetCountIsCalledAfterDisposal()
		{
			//setup
			var dal = Disposed();

			//act
			//assert
			Should.Throw<ObjectDisposedException>(() => dal.GetCount<Company>());
		}

		[Fact]
		public void ShouldThrowObjectDisposedExceptionWhenGetIsCalledAfterDisposal()
		{
			//setup
			var dal = Disposed();

			//act
			//assert
			Should.Throw<ObjectDisposedException>(() => dal.Get<Company>(1));
		}

		[Fact]
		public void ShouldThrowObjectDisposedExceptionWhenInsertIsCalledAfterDisposal()
		{
			//setup
			var dal = Disposed();

			//act
			//assert
			Should.Throw<ObjectDisposedException>(() => dal.Insert(new Company { Name = "Acme" }));
		}

		[Fact]
		public void ShouldThrowObjectDisposedExceptionWhenUpdateIsCalledAfterDisposal()
		{
			//setup
			var dal = Disposed();

			//act
			//assert
			Should.Throw<ObjectDisposedException>(() => dal.Update(new Company { Name = "Acme" }));
		}

		[Fact]
		public void ShouldThrowObjectDisposedExceptionWhenDeleteIsCalledAfterDisposal()
		{
			//setup
			var dal = Disposed();

			//act
			//assert
			Should.Throw<ObjectDisposedException>(() => dal.Delete(new Company { Name = "Acme" }));
		}

		[Fact]
		public void ShouldThrowObjectDisposedExceptionWhenTransactionStartIsCalledAfterDisposal()
		{
			//setup
			var dal = Disposed();

			//act
			//assert
			Should.Throw<ObjectDisposedException>(() => dal.TransactionStart());
		}

		[Fact]
		public void ShouldThrowObjectDisposedExceptionWhenTransactionCommitIsCalledAfterDisposal()
		{
			//setup
			var dal = Disposed();

			//act
			//assert
			Should.Throw<ObjectDisposedException>(() => dal.TransactionCommit());
		}

		[Fact]
		public void ShouldThrowObjectDisposedExceptionWhenTransactionRollBackIsCalledAfterDisposal()
		{
			//setup
			var dal = Disposed();

			//act
			//assert
			Should.Throw<ObjectDisposedException>(() => dal.TransactionRollBack());
		}

		/// <summary>
		/// The contract makes this an explicit guarantee rather than an incidental consequence of the framework's
		/// type hierarchy, so it is caught by hand here rather than through an assertion helper.
		/// </summary>
		[Fact]
		public void ShouldAllowUseAfterDisposalToBeCaughtAsInvalidOperationException()
		{
			//setup
			var dal = Disposed();
			Exception caught = null;

			//act
			try
			{
				dal.GetCount<Company>();
			}
			catch (InvalidOperationException ex)
			{
				caught = ex;
			}

			//assert
			caught.ShouldNotBeNull();
			caught.ShouldBeOfType<ObjectDisposedException>();
		}

		[Fact]
		public void ShouldAllowATransactionMisuseAndUseAfterDisposalToBeCaughtByTheSameHandler()
		{
			//setup
			var open = new ConformingDataAccess();
			var disposed = Disposed();
			var caught = 0;

			//act
			foreach (var call in new Action[] { () => open.TransactionCommit(), () => disposed.GetCount<Company>() })
			{
				try
				{
					call();
				}
				catch (InvalidOperationException)
				{
					caught++;
				}
			}

			//assert
			caught.ShouldBe(2);
		}

		private static ConformingDataAccess Disposed()
		{
			var dal = new ConformingDataAccess();
			dal.Dispose();
			return dal;
		}
	}
}
