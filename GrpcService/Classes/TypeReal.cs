namespace GrpcService.Classes
{
    public class TypeReal : Type
    {
        public new bool Validate(string value)
        {
            double buf;
            if (double.TryParse(value, out buf))
                return true;
            return false;
        }
    }
}
