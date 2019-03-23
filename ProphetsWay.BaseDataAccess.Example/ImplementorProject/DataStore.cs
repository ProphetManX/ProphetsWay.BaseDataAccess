using ProphetsWay.BaseDataAccess.Example.Entities;
using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess.Example.ImplementorProject
{
    /// <summary>
    /// Ignore this class, I created it to be an in-memory database for use in the example.
    /// 
    /// In your implementation, this could be considered your database context (ex: Entity Framework implementation)
    /// </summary>
    internal static class DataStore
    {
        public static readonly Dictionary<int, Job> Jobs = new Dictionary<int, Job>();

        public static readonly Dictionary<int, Company> Companies = new Dictionary<int, Company>();

        public static readonly Dictionary<int, User> Users = new Dictionary<int, User>();
    }
}
