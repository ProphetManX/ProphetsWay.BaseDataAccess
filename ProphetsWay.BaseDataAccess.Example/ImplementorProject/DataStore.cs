using ProphetsWay.BaseDataAccess.Example.Entities;
using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Example.ImplementorProject
{
    internal static class DataStore
    {
        public static readonly Dictionary<int, Job> Jobs = new Dictionary<int, Job>();

        public static readonly Dictionary<int, Company> Companies = new Dictionary<int, Company>();

        public static readonly Dictionary<int, User> Users = new Dictionary<int, User>();
    }
}
