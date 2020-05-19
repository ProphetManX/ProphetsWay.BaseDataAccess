using System;
using System.Collections.Generic;
using ProphetsWay.BaseDataAccess.Example.Entities;
using ProphetsWay.BaseDataAccess.Example.IDaos;
using ProphetsWay.BaseDataAccess.Example.ImplementorProject.Daos;

namespace ProphetsWay.BaseDataAccess.Example.ImplementorProject
{
    /// <summary>
    /// This is the main entry point for the DAL implementation.  In this example, each of the individual DAOs 
    /// are created internally and each call is mapped to the internal DAO
    /// This class has hardly any functional/logical code within it
    /// 
    /// If you choose to do so, you can put all your actual code within this one file and not bother with each separate DAO
    /// but that is not recommended
    /// </summary>
    public class ExampleDataAccess : BaseDataAccess<int>, IExampleDataAccess
    {
        private readonly ICompanyDao _companyDao = new CompanyDao();
        private readonly IJobDao _jobDao = new JobDao();
        private readonly IUserDao _userDao = new UserDao();

        public void CustomUserFunctionality(User user)
        {
            _userDao.CustomUserFunctionality(user);
        }

        public int Delete(Company item)
        {
            return _companyDao.Delete(item);
        }

        public int Delete(Job item)
        {
            return _jobDao.Delete(item);
        }

        public int Delete(User item)
        {
            return _userDao.Delete(item);
        }

        public Company Get(Company item)
        {
            return _companyDao.Get(item);
        }

        public Job Get(Job item)
        {
            return _jobDao.Get(item);
        }

        public User Get(User item)
        {
            return _userDao.Get(item);
        }

        public IList<Job> GetAll(Job item)
        {
            return _jobDao.GetAll(item);
        }

        public int GetCount(Company item)
        {
            return _companyDao.GetCount(item);
        }

        public Company GetCustomCompanyFunction(int id)
        {
            return _companyDao.GetCustomCompanyFunction(id);
        }

        public IList<Company> GetPaged(Company item, int skip, int take)
        {
            return _companyDao.GetPaged(item, skip, take);
        }

        public void Insert(Company item)
        {
            _companyDao.Insert(item);
        }

        public void Insert(Job item)
        {
            _jobDao.Insert(item);
        }

        public void Insert(User item)
        {
            _userDao.Insert(item);
        }

        public override void TransactionCommit()
        {
            //not implementing these for the example, but you could do something like "context.CommitTransaction"
            throw new NotImplementedException();
        }

        public override void TransactionRollBack()
        {
            //not implementing these for the example, but you could do something like "context.RollbackTransaction"
            throw new NotImplementedException();
        }

        public override void TransactionStart()
        {
            //not implementing these for the example, but you could do something like "context.BeginTransaction"
            throw new NotImplementedException();
        }

        public int Update(Company item)
        {
            return _companyDao.Update(item);
        }

        public int Update(Job item)
        {
            return _jobDao.Update(item);
        }

        public int Update(User item)
        {
            return _userDao.Update(item);
        }
    }
}
