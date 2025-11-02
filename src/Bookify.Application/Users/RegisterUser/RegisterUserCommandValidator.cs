using FluentValidation;

namespace Bookify.Application.Users.RegisterUser
{
	internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
	{
		public RegisterUserCommandValidator()
		{
			RuleFor(x => x.FirstName)
				.NotEmpty();

			RuleFor(x => x.LastName)
				.NotEmpty();

			RuleFor(x => x.Email)
				.EmailAddress();

			RuleFor(x => x.Password)
				.NotEmpty()
				.MinimumLength(5);
		}
	}
}
