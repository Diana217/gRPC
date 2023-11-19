using System.Text.RegularExpressions;

namespace GrpcService.Classes
{
    public class TypeEmail : Type
    {
        public new bool Validate(string value)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (value == null)
                return false;
            return Regex.IsMatch(value, pattern);
        }
    }
}
