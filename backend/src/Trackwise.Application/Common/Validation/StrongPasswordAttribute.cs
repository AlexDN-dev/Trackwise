using System.ComponentModel.DataAnnotations;

namespace Trackwise.Application.Common.Validation;

public class StrongPasswordAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string password)
            return new ValidationResult("Mot de passe invalide.");

        var manquants = new List<string>();
        
        if(password.Length < 8)
            manquants.Add("au moins 8 caractères");
        if(!password.Any(char.IsUpper))
            manquants.Add("une majuscule");
        if(!password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
            manquants.Add("un symbole");

        return manquants.Count == 0
            ? ValidationResult.Success
            : new ValidationResult($"Le mot de passe doit contenir : {string.Join(", ", manquants)}.");
    }
}