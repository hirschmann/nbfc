namespace StagWare.FanControl.Configurations.Validation
{
    public enum ValidationResult
    {
        Success,
        Warning,
        Error
    }

    public interface IValidationRule<T>
    {
        string Description { get; }
        ValidationResult Validate(T item);
    }
}