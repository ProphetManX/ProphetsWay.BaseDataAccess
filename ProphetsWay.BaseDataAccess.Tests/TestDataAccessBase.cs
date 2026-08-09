namespace ProphetsWay.BaseDataAccess.Tests
{
	/// <summary>
	/// Supplies the transaction members every concrete <see cref="BaseDataAccess"/> must implement, so the
	/// test doubles below contain nothing but the convention method under examination.
	/// </summary>
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
	}
}
