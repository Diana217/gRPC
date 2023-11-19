namespace GrpcService.Classes
{
    public class TypeString : Type
    {
        public new bool Validate(string value)
        {
            return true;
        }
    }
}
