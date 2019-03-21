using ProphetsWay.BaseDataAccess.Example.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProphetsWay.BaseDataAccess.Example.IDaos
{
    public interface IUserDao : IBaseDao<User>
    {
        void CustomUserFunctionality(User user);
    }
}
