namespace GrpcService.Classes
{
    public class Database
    {
        public int Id { get; set; }
        public string DBName { get; set; }
        public string DBPath { get; set; }
        public List<Table> Tables { get; set; }

        public Database() { }
        public Database(string name)
        {
            DBName = name;
            Tables = new List<Table>();
        }
    }
}
