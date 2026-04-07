using GSDT.MasterData.Entities;
using FluentAssertions;

namespace GSDT.MasterData.Tests.Entities;

/// <summary>
/// TC-MD-D002: Province/District/Ward hierarchy — verify codes link correctly across levels.
/// </summary>
public sealed class ProvinceDistrictWardHierarchyTests
{
    [Fact]
    public void Province_District_Ward_HierarchyCodes_LinkCorrectly()
    {
        var province = Province.Create("HCM", "TP. Hồ Chí Minh", "Ho Chi Minh City");
        var district = District.Create("Q1", province.Code, "Quận 1", "District 1");
        var ward = Ward.Create("W01", district.Code, "Phường Bến Nghé", "Ben Nghe Ward");

        district.ProvinceCode.Should().Be(province.Code);
        ward.DistrictCode.Should().Be(district.Code);
    }

    [Fact]
    public void Province_Create_SetsCorrectFields()
    {
        var province = Province.Create("DN", "Đà Nẵng", "Da Nang", sortOrder: 2);

        province.Code.Should().Be("DN");
        province.NameVi.Should().Be("Đà Nẵng");
        province.NameEn.Should().Be("Da Nang");
        province.SortOrder.Should().Be(2);
        province.IsActive.Should().BeTrue();
    }

    [Fact]
    public void District_Create_SetsCorrectFields()
    {
        var district = District.Create("HK", "HN", "Hoàn Kiếm", "Hoan Kiem", sortOrder: 1);

        district.Code.Should().Be("HK");
        district.ProvinceCode.Should().Be("HN");
        district.NameVi.Should().Be("Hoàn Kiếm");
        district.NameEn.Should().Be("Hoan Kiem");
        district.SortOrder.Should().Be(1);
        district.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Ward_Create_SetsCorrectFields()
    {
        var ward = Ward.Create("HG", "HK", "Phường Hàng Gai", "Hang Gai Ward", sortOrder: 5);

        ward.Code.Should().Be("HG");
        ward.DistrictCode.Should().Be("HK");
        ward.NameVi.Should().Be("Phường Hàng Gai");
        ward.NameEn.Should().Be("Hang Gai Ward");
        ward.SortOrder.Should().Be(5);
        ward.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Province_District_Ward_AllGenerateUniqueIds()
    {
        var province = Province.Create("HN", "Hà Nội", "Ha Noi");
        var district = District.Create("HK", "HN", "Hoàn Kiếm", "Hoan Kiem");
        var ward = Ward.Create("HG", "HK", "Hàng Gai", "Hang Gai");

        province.Id.Should().NotBe(Guid.Empty);
        district.Id.Should().NotBe(Guid.Empty);
        ward.Id.Should().NotBe(Guid.Empty);

        // All three IDs must be distinct
        new[] { province.Id, district.Id, ward.Id }.Distinct().Should().HaveCount(3);
    }
}
