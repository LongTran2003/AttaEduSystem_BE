using System.ComponentModel.DataAnnotations;

namespace AttaEduSystem.Utilities.ValidationAttribute
{
    public class ConfirmPasswordAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        private readonly string _passwordPropertyName;

        public ConfirmPasswordAttribute(string passwordPropertyName)
        {
            _passwordPropertyName = passwordPropertyName;
            ErrorMessage = "The confirmation password does not match the original password.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Lấy giá trị của thuộc tính Password từ model
            var passwordProperty = validationContext.ObjectType.GetProperty(_passwordPropertyName);
            if (passwordProperty == null)
            {
                throw new ArgumentException($"Property '{_passwordPropertyName}' not found.");
            }

            var passwordValue = (string)passwordProperty.GetValue(validationContext.ObjectInstance)!;

            // Kiểm tra xem ConfirmPassword có giống với Password không
            if (value is string confirmPassword && confirmPassword != passwordValue)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
