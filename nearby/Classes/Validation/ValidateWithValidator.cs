using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nearby.Classes.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidateWithValidator : ValidationAttribute
    {
        private string ValidatorName;
        public ValidateWithValidator(string validatorName)
        {
            ValidatorName = validatorName;
        }
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is not string str)
                return new ValidationResult("Ожидается строка");

            var prop = typeof(Validate).GetProperty(ValidatorName, BindingFlags.Static | BindingFlags.Public);
            var validator = prop?.GetValue(null) as Func<string, bool>;

            if (validator == null)
                throw new InvalidOperationException($"Правило '{ValidatorName}' не найдено или имеет неверный тип");

            return validator(str)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? $"Значение не соответствует правилу {ValidatorName}");
        }
    }
}
