using System.Text.RegularExpressions;

namespace CRUDTable.Validators
{
    public class DateValidator : IValidator
    {

        protected static Regex r = new Regex("^\\d{2,4}[/\\-]\\d{2}[/\\-]\\d{2,4}$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Validates the specified string.
        /// </summary>
        /// <param name="dirty">The string.</param>
        /// <returns></returns>
        public bool Validate(string dirty)
        {
            return r.IsMatch(dirty);
        }

    }
}
