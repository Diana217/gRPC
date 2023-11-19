namespace GrpcService.Classes
{
    public class TypeChar : Type
    {
        public new bool Validate(string value)
        {
            char buf;
            if (char.TryParse(value, out buf))
                return true;
            return false;
        }
    }
}
