using System.ComponentModel.DataAnnotations;
using Cronos;

namespace Rescheduler.Api.Models;

public class ValidateCronAttribute : ValidationAttribute
{
    private string GetErrorMessage(string? cron) =>
        $"Invalid cron expression {cron}";

    protected override ValidationResult? IsValid(
        object? value, ValidationContext validationContext)
    {
        var cron = (string?)value;
        
        if (String.IsNullOrWhiteSpace(cron) || !CronExpression.TryParse(cron, out _))
        {
            return new ValidationResult(GetErrorMessage(cron));
        }
        
        return ValidationResult.Success;
    }
}