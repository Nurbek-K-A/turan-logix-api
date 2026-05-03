using FluentAssertions;
using TuranLogix.Domain.Entities;
using TuranLogix.Domain.Enums;

namespace TuranLogix.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_ShouldSetDefaultRole_Client()
    {
        var user = User.Create("Иван Иванов", "ivan@test.kz", "+77001234567", "hashedpw");

        user.Role.Should().Be(UserRole.Client);
        user.IsVerified.Should().BeFalse();
    }

    [Fact]
    public void Create_WithCompanyInfo_ShouldSetBin()
    {
        var user = User.Create("Иван Иванов", "ivan@test.kz", "+77001234567", "hashedpw",
            companyName: "ТОО Тест", bin: "123456789012");

        user.CompanyName.Should().Be("ТОО Тест");
        user.Bin.Should().Be("123456789012");
    }

    [Fact]
    public void Verify_ShouldSetIsVerifiedTrue()
    {
        var user = User.Create("Иван Иванов", "ivan@test.kz", "+77001234567", "hashedpw");
        user.Verify();

        user.IsVerified.Should().BeTrue();
    }
}
