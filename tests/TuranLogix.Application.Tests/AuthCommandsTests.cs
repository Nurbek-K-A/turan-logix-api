using FluentAssertions;
using Moq;
using TuranLogix.Application.Common.Interfaces;
using TuranLogix.Application.Features.Auth.Commands;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Interfaces;

namespace TuranLogix.Application.Tests;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldReturnFailure()
    {
        var existingUser = User.Create("Тест", "existing@test.kz", "+77001234567", "hash");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("existing@test.kz", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new RegisterCommandHandler(_userRepositoryMock.Object, _unitOfWorkMock.Object, _passwordHasherMock.Object);
        var command = new RegisterCommand("Иван", "existing@test.kz", "+77001234567", "Password123", null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmailAlreadyExists");
    }

    [Fact]
    public async Task Handle_WhenNewEmail_ShouldRegisterUser()
    {
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("new@test.kz", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(h => h.Hash("Password123")).Returns("hashed_pw");
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new RegisterCommandHandler(_userRepositoryMock.Object, _unitOfWorkMock.Object, _passwordHasherMock.Object);
        var command = new RegisterCommand("Иван Иванов", "new@test.kz", "+77001234567", "Password123", null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
