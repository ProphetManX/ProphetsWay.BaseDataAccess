namespace ProphetsWay.BaseDataAccess.Example.Entities
{
    public class Job : IBaseDDLEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Something { get; set; }

    }
}
