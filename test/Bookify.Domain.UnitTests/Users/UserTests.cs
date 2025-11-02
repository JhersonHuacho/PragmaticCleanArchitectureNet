using Bookify.Domain.UnitTests.Infrastructure;
using Bookify.Domain.Users;
using Bookify.Domain.Users.Events;
using FluentAssertions;

namespace Bookify.Domain.UnitTests.Users
{
	public class UserTests : BaseTest
	{
		[Fact]
		public void Create_Should_SetPropertyValues()
		{
			// Arrange
			// Act
			var user = User.Create(UserData.FirstName, UserData.LastName, UserData.Email);
			
			// Assert
			user.FirstName.Should().Be(UserData.FirstName);
			user.LastName.Should().Be(UserData.LastName);
			user.Email.Should().Be(UserData.Email);
		}

		[Fact]
		public void Create_Should_RaiseUserCreateDomainEvent()
		{
			// Arrange
			// Act
			var user = User.Create(UserData.FirstName, UserData.LastName, UserData.Email);
			
			// Assert
			var domainEvent = user.GetDomainEvents().OfType<UserCreatedDomainEvents>().FirstOrDefault();

			domainEvent.UserId.Should().Be(user.Id);
		}

		[Fact]
		public void Create_Should_RaiseUserCreateDomainEvent_V2()
		{
			// Arrange
			// Act
			var user = User.Create(UserData.FirstName, UserData.LastName, UserData.Email);

			// Assert
			var domainEvent = AssertDomainEventWasPublished<UserCreatedDomainEvents>(user);

			domainEvent.UserId.Should().Be(user.Id);
		}

		[Fact]
		public void Create_Should_AddRegisteredRoleToUser()
		{
			// Arrange
			// Act
			var user = User.Create(UserData.FirstName, UserData.LastName, UserData.Email);
			
			// Assert
			user.Roles.Should().Contain(Role.Registered);
		}
	}
}
