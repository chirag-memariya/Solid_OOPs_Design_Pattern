using FluentValidation;

public class StudentValidator : AbstractValidator<StudentDto>
{
    public StudentValidator()
    {
        RuleFor(x=>x.Name).NotEmpty().MinimumLength(3);

        RuleFor(x=>x.Email).NotEmpty().EmailAddress();
    }
}