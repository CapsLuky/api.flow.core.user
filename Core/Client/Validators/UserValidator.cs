using Core.Client.Models;
using FluentValidation;

namespace Core.Client.Validators;

public class UserValidator: AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .WithErrorCode("NotEmptyValidator");
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required")
            .WithErrorCode("NotEmptyValidator");
    }
}