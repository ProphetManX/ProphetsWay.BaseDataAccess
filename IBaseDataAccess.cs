using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess
{
	public interface IBaseDataAccess<TGenericDDLItem> : IBaseDataAccess, IBaseDao<TGenericDDLItem>
		where TGenericDDLItem : BaseDDLEntity
	{
		IList<T> GetAll<T>() where T : TGenericDDLItem, new();
	}

	public interface IBaseDataAccess
	{
		T Get<T>(long id) where T : BaseEntity, new();

		T Get<T>(int id) where T : BaseEntity, new();

		IList<T> GetAll<T>(T item) where T : BaseEntity, new();

		void TransactionStart();

		void TransactionCommit();

		void TransactionRollBack();
	}

	
}