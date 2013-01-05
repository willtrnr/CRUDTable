using System.Text.RegularExpressions;

namespace CRUDTable.Validators
{
    public class AlphaValidator : IValidator
    {

        protected static Regex r = new Regex("^[A-Za-z]+$", RegexOptions.IgnoreCase);

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
