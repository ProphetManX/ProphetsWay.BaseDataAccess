using ProphetsWay.BaseDataAccess.Example.Entities;
using ProphetsWay.BaseDataAccess.Example.IDaos;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.BaseDataAccess.Example.ImplementorProject.Daos
{
    internal class JobDao : IJobDao
    {
        public int Delete(Job item)
        {
            DataStore.Jobs.Remove(item.Id);
            return 1;
        }

        public Job Get(Job item)
        {
            if (DataStore.Jobs.ContainsKey(item.Id))
                return DataStore.Jobs[item.Id];

            return null;
        }

        public IList<Job> GetAll(Job item)
        {
            return DataStore.Jobs.Values.ToList();
        }

        public void Insert(Job item)
        {
            item.Id = DataStore.Jobs.Keys.Count > 0
                ? DataStore.Jobs.Keys.Max() + 1
                : 1;

            DataStore.Jobs.Add(item.Id, item);
        }

        public int Update(Job item)
        {
            DataStore.Jobs[item.Id] = item;
            return 1;
        }
    }
}
