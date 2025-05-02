using System;

namespace ProphetsWay.BaseDataAccess
{
    public interface IBaseSoftEntity : IBaseEntity
    {
        DateTime CreatedDate { get; set; }
        
        DateTime? UpdatedDate { get; set; }
        
        DateTime DeletedDate { get; set; }
    }
}
