using System.Text.RegularExpressions;

namespace CRUDTable.Validators
{
    public class NotNullValidator : IValidator
    {

        protected static Regex r = new Regex("^\\s+$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Validates the specified string.
        /// </summary>
        /// <param name="dirty">The string.</param>
        /// <returns></returns>
        public bool Validate(string dirty)
        {
            return (dirty != null && dirty != "" && !r.IsMatch(dirty));
        }

    }
}
