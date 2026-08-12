using System;
using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// A hand-written Data Access Layer that implements <see cref="IBaseDataAccess"/> directly and obeys the
	/// disposal and transaction rules stated on it. <b>This class is the subject of
	/// <see cref="ConformingDataAccessDisposalTests"/> and <see cref="ConformingDataAccessTransactionTests"/> -
	/// <see cref="BaseDataAccess"/> is not.</b>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every rule those two classes assert is an obligation on the <i>implementer</i> of
	/// <see cref="IBaseDataAccess"/>. <see cref="BaseDataAccess"/> declares <c>Dispose</c> and all three
	/// transaction members <c>abstract</c>, and holds no connection, context or transaction state of its own, so
	/// it cannot enforce any of them - it only requires that an implementer decide. This class is that decision,
	/// made correctly, so the contract has something concrete to be asserted against. Nothing here is inherited
	/// from the library, and a different implementation that got these rules wrong would not be caught by these
	/// tests - only by its own.
	/// </para>
	/// <para>
	/// The storage model is deliberately thin. <see cref="Insert{TEntityType}"/> is the only member that writes,
	/// because "disposal rolls back, never commits" needs exactly one observable write path to be provable
	/// through data rather than through a flag the double sets about itself. The read members,
	/// <see cref="Update{TEntityType}"/> and <see cref="Delete{TEntityType}"/> are implemented only far enough
	/// for the "throws once disposed" rule to be asserted on every member of the interface; they are not a
	/// specification of retrieval or row-count semantics, and no test should read them as one.
	/// </para>
	/// </remarks>
	public class ConformingDataAccess : IBaseDataAccess
	{
		public const string CommitFailureMessage = "The store refused the commit.";
		public const string RollBackFailureMessage = "The store refused the roll back.";
		public const string NoTransactionOpenMessage = "No transaction is open on this instance.";
		public const string TransactionAlreadyOpenMessage = "A transaction is already open on this instance.";

		private readonly IList<IBaseEntity> _committed = new List<IBaseEntity>();
		private readonly IList<IBaseEntity> _pending = new List<IBaseEntity>();

		private bool _disposed;
		private bool _transactionOpen;

		/// <summary>
		/// Set before the call under test to make the next <see cref="TransactionCommit"/> fail after it has
		/// closed the transaction, modelling a commit the store rejects.
		/// </summary>
		public bool CommitShouldFail;

		/// <summary>
		/// Set before the call under test to make the next <see cref="TransactionRollBack"/> fail after it has
		/// closed the transaction, modelling a roll back the store rejects.
		/// </summary>
		public bool RollBackShouldFail;

		/// <summary>Counts every call to <see cref="Dispose"/>, including the ones that are no-ops.</summary>
		public int DisposeCallCount;

		/// <summary>Counts commit attempts, incremented before a commit that is going to fail.</summary>
		public int CommitAttemptCount;

		/// <summary>Counts roll back attempts, incremented before a roll back that is going to fail.</summary>
		public int RollBackAttemptCount;

		/// <summary>Entities the store has actually committed.</summary>
		public IList<IBaseEntity> Committed
		{
			get { return _committed; }
		}

		/// <summary>Entities written inside the transaction currently open, not yet committed.</summary>
		public IList<IBaseEntity> Pending
		{
			get { return _pending; }
		}

		public bool IsDisposed
		{
			get { return _disposed; }
		}

		public bool TransactionIsOpen
		{
			get { return _transactionOpen; }
		}

		public IList<T> GetAll<T>() where T : IBaseEntity
		{
			ThrowIfDisposed();

			return CommittedOfType<T>();
		}

		public IList<T> GetPaged<T>(int skip, int take) where T : IBaseEntity
		{
			ThrowIfDisposed();

			var all = CommittedOfType<T>();
			var page = new List<T>();

			for (var i = skip; i < all.Count && page.Count < take; i++)
				page.Add(all[i]);

			return page;
		}

		public int GetCount<T>() where T : IBaseEntity
		{
			ThrowIfDisposed();

			return CommittedOfType<T>().Count;
		}

		public T Get<T>(object id) where T : IBaseEntity, new()
		{
			ThrowIfDisposed();

			var all = CommittedOfType<T>();
			return all.Count == 0 ? default(T) : all[0];
		}

		public void Insert<TEntityType>(TEntityType item) where TEntityType : IBaseEntity, new()
		{
			ThrowIfDisposed();

			//a write made with no transaction open auto-commits on its own
			if (_transactionOpen)
				_pending.Add(item);
			else
				_committed.Add(item);
		}

		public int Update<TEntityType>(TEntityType item) where TEntityType : IBaseEntity, new()
		{
			ThrowIfDisposed();

			return 1;
		}

		public int Delete<TEntityType>(TEntityType item) where TEntityType : IBaseEntity, new()
		{
			ThrowIfDisposed();

			return 1;
		}

		public void TransactionStart()
		{
			ThrowIfDisposed();

			if (_transactionOpen)
				throw new InvalidOperationException(TransactionAlreadyOpenMessage);

			_transactionOpen = true;
		}

		public void TransactionCommit()
		{
			ThrowIfDisposed();

			if (!_transactionOpen)
				throw new InvalidOperationException(NoTransactionOpenMessage);

			CommitAttemptCount++;

			//closed before the outcome is known, so a failed commit leaves no transaction open either
			_transactionOpen = false;

			if (CommitShouldFail)
			{
				_pending.Clear();
				throw new TransactionFailureException(CommitFailureMessage);
			}

			foreach (var item in _pending)
				_committed.Add(item);

			_pending.Clear();
		}

		public void TransactionRollBack()
		{
			ThrowIfDisposed();

			if (!_transactionOpen)
				throw new InvalidOperationException(NoTransactionOpenMessage);

			RollBackAttemptCount++;
			_transactionOpen = false;
			_pending.Clear();

			if (RollBackShouldFail)
				throw new TransactionFailureException(RollBackFailureMessage);
		}

		public void Dispose()
		{
			DisposeCallCount++;

			if (_disposed)
				return;

			if (_transactionOpen)
			{
				try
				{
					TransactionRollBack();
				}
				catch (Exception)
				{
					//a roll back that fails during disposal is swallowed; Dispose is forbidden to throw
				}
			}

			_disposed = true;
		}

		private List<T> CommittedOfType<T>()
		{
			var results = new List<T>();

			foreach (var item in _committed)
			{
				if (item is T)
					results.Add((T)item);
			}

			return results;
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(typeof(ConformingDataAccess).Name);
		}
	}
}
