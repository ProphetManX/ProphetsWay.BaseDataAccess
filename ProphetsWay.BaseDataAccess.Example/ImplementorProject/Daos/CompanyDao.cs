using ProphetsWay.BaseDataAccess.Example.Entities;
using ProphetsWay.BaseDataAccess.Example.IDaos;
using System.Collections.Generic;
using System.Linq;

namespace ProphetsWay.BaseDataAccess.Example.ImplementorProject.Daos
{
    internal class CompanyDao : ICompanyDao
    {
        public int Delete(Company item)
        {
            DataStore.Companies.Remove(item.Id);
            return 1;
        }

        public Company Get(Company item)
        {
            if (DataStore.Companies.ContainsKey(item.Id))
                return DataStore.Companies[item.Id];

            return null;
        }

        public Company GetCustomCompanyFunction(int id)
        {
            var index = id % DataStore.Companies.Count;
            return DataStore.Companies[index];
        }

        public IList<Company> GetPaged(Company item, int skip, int take)
        {
            return DataStore.Companies.Values.Skip(skip).Take(take).ToList();
        }

        public void Insert(Company item)
        {
            item.Id = DataStore.Companies.Keys.Max() + 1;
            DataStore.Companies.Add(item.Id, item);
        }

        public int Update(Company item)
        {
            DataStore.Companies[item.Id] = item;
            return 1;
        }
    }
}
