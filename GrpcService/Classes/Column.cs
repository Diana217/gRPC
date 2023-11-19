using Google.Protobuf;

namespace GrpcService.Classes
{
    public class Column
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public string ColName { get; set; }
        public int TypeId { get; set; }
        public Type ColType { get; set; }

        public Column() { }
        public Column(string name, string type)
        {
            ColName = name;

            switch (type)
            {
                case "Integer":
                    ColType = new TypeInteger();
                    break;
                case "Real":
                    ColType = new TypeReal();
                    break;
                case "Char":
                    ColType = new TypeChar();
                    break;
                case "String":
                    ColType = new TypeString();
                    break;
                case "Enum":
                    ColType = new TypeEnum();
                    break;
                case "Email":
                    ColType = new TypeEmail();
                    break;
                default:
                    ColType = new TypeString();
                    break;
            }
        }
    }
}
