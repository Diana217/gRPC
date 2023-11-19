namespace GrpcService.Classes
{
    public class TypeInteger : Type
    {
        public new bool Validate(string value)
        {
            int buf;
            if (int.TryParse(value, out buf))
                return true;
            return false;
        }
    }
}
