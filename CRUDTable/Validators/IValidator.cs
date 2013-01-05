namespace CRUDTable.Validators
{
    public interface IValidator
    {

        /// <summary>
        /// Validates the specified string.
        /// </summary>
        /// <param name="dirty">The string.</param>
        /// <returns></returns>
        bool Validate(string dirty);

    }
}
