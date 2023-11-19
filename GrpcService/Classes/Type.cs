namespace GrpcService.Classes
{
    public class Type
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Validate(string value) => true;
    }
}
