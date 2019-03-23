using ProphetsWay.BaseDataAccess.Example.Entities;
using ProphetsWay.BaseDataAccess.Example.IDaos;
using System.Linq;

namespace ProphetsWay.BaseDataAccess.Example.ImplementorProject.Daos
{
    internal class UserDao : IUserDao
    {
        public void CustomUserFunctionality(User user)
        {
            //this example function is silly, but just used to illustrate some sort of custom query/function in your DAL
            DataStore.Users[user.Id].Whatever = "custom functionality triggered";
        }

        public int Delete(User item)
        {
            DataStore.Users.Remove(item.Id);
            return 1;
        }

        public User Get(User item)
        {
            if (DataStore.Users.ContainsKey(item.Id))
                return DataStore.Users[item.Id];

            return null;
        }

        public void Insert(User item)
        {
            item.Id = DataStore.Users.Keys.Count > 0
                ? DataStore.Users.Keys.Max() + 1
                : 1;

            DataStore.Users.Add(item.Id, item);
        }

        public int Update(User item)
        {
            DataStore.Users[item.Id] = item;
            return 1;
        }
    }
}
