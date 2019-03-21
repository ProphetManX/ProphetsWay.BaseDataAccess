using System;
using System.Collections.Generic;
using System.Text;

namespace ProphetsWay.BaseDataAccess.Example.Entities
{
    public class User : IBaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Company Company { get; set; }

        public Job Job { get; set; }

        public string  Whatever { get; set; }
    }
}
