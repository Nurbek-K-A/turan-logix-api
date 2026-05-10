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

    // --- Update / phone verification reset ---

    [Fact]
    public void Update_WithNewPhoneNumber_ShouldResetIsPhoneVerifiedAndClearPending()
    {
        var user = User.Create("Иван Иванов", "ivan@test.kz", "+77001111111", "hashedpw");
        user.MarkPhoneVerified();
        user.SetPendingVerification("verify-old", "+77001111111", 300);

        user.Update("Иван Иванов", "+77009999999", null, null);

        user.PhoneNumber.Should().Be("+77009999999");
        user.IsPhoneVerified.Should().BeFalse();
        user.PendingVerifyId.Should().BeNull();
        user.PendingVerifyPhone.Should().BeNull();
        user.PendingVerifyExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Update_WithSamePhoneNumber_ShouldNotResetIsPhoneVerified()
    {
        var user = User.Create("Иван Иванов", "ivan@test.kz", "+77001111111", "hashedpw");
        user.MarkPhoneVerified();

        user.Update("Иван Петров", "+77001111111", null, null);

        user.IsPhoneVerified.Should().BeTrue();
    }

    // --- SetPendingVerification / ClearPendingVerification ---

    [Fact]
    public void SetPendingVerification_ShouldPopulateAllThreeFields()
    {
        var user = User.Create("Иван Иванов", "ivan@test.kz", "+77001234567", "hashedpw");
        var before = DateTime.UtcNow;

        user.SetPendingVerification("verify-123", "+77001234567", 300);

        user.PendingVerifyId.Should().Be("verify-123");
        user.PendingVerifyPhone.Should().Be("+77001234567");
        user.PendingVerifyExpiresAt.Should().BeCloseTo(before.AddSeconds(300), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ClearPendingVerification_AfterSet_ShouldNullAllThreeFields()
    {
        var user = User.Create("Иван Иванов", "ivan@test.kz", "+77001234567", "hashedpw");
        user.SetPendingVerification("verify-123", "+77001234567", 300);

        user.ClearPendingVerification();

        user.PendingVerifyId.Should().BeNull();
        user.PendingVerifyPhone.Should().BeNull();
        user.PendingVerifyExpiresAt.Should().BeNull();
    }
}
