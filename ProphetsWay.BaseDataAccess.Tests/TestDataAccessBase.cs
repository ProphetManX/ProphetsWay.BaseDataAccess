namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Supplies the transaction members and <see cref="System.IDisposable.Dispose"/> that every concrete
	/// <see cref="BaseDataAccess"/> must implement, so the test doubles below contain nothing but the
	/// convention method under examination.
	/// </summary>
	/// <remarks>
	/// The bodies here are empty on purpose rather than by reflex: doubles deriving from this class exist to
	/// exercise the reflection dispatch in <see cref="BaseDataAccess"/>, and hold no connection, context or
	/// transaction state for any of these four members to act on. The disposal and transaction rules stated on
	/// <see cref="IBaseDataAccess"/> are obligations on an implementer, and are asserted against
	/// <see cref="ConformingDataAccess"/> - a double built for that job - not against these.
	/// </remarks>
	public abstract class TestDataAccessBase : BaseDataAccess
	{
		public override void TransactionStart()
		{
		}

		public override void TransactionCommit()
		{
		}

		public override void TransactionRollBack()
		{
		}

		public override void Dispose()
		{
		}
	}
}
