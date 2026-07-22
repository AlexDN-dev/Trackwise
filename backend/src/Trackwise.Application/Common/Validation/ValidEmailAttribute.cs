using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Trackwise.Application.Common.Validation;

public sealed class ValidEmailAttribute : ValidationAttribute
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is string email && EmailRegex.IsMatch(email))
            return ValidationResult.Success;

        return new ValidationResult(
            $"Le champ {validationContext.DisplayName} doit etre une adresse email valide.");
    }
}