
namespace GSDT.MasterData.Infrastructure.Services;

/// <summary>
/// Seeds reference data at startup (idempotent — COUNT check, synchronous on StartAsync so app waits).
/// Also loads Province/District/Ward into IMemoryCache NeverRemove after seeding.
/// </summary>
public class MasterDataSeeder(
    IServiceScopeFactory scopeFactory,
    ILogger<MasterDataSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MasterDataDbContext>();
        var adminUnitService = scope.ServiceProvider.GetRequiredService<AdministrativeUnitService>();

        await db.Database.EnsureCreatedAsync(ct);

        await SeedProvincesAsync(db, ct);
        await SeedAdministrativeUnitsAsync(db, ct);
        await SeedCaseTypesAsync(db, ct);
        await SeedJobTitlesAsync(db, ct);

        await adminUnitService.BuildCacheAsync(ct);
        await LoadHotDataCacheAsync(scope.ServiceProvider, db, ct);

        logger.LogInformation("MasterData seeding and cache loading complete");
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private static async Task SeedProvincesAsync(MasterDataDbContext db, CancellationToken ct)
    {
        if (await db.Provinces.AnyAsync(ct)) return;

        var provinces = new[]
        {
            Province.Create("01", "Hà Nội", "Hanoi", 1),
            Province.Create("79", "Thành phố Hồ Chí Minh", "Ho Chi Minh City", 2),
            Province.Create("48", "Đà Nẵng", "Da Nang", 3),
            Province.Create("92", "Cần Thơ", "Can Tho", 4),
            Province.Create("31", "Hải Phòng", "Hai Phong", 5),
        };

        var districts = new[]
        {
            District.Create("001", "01", "Quận Ba Đình", "Ba Dinh District", 1),
            District.Create("002", "01", "Quận Hoàn Kiếm", "Hoan Kiem District", 2),
            District.Create("003", "01", "Quận Tây Hồ", "Tay Ho District", 3),
        };

        var wards = new[]
        {
            Ward.Create("00001", "001", "Phường Phúc Xá", "Phuc Xa Ward", 1),
            Ward.Create("00004", "001", "Phường Trúc Bạch", "Truc Bach Ward", 2),
            Ward.Create("00006", "001", "Phường Vĩnh Phúc", "Vinh Phuc Ward", 3),
        };

        await db.Provinces.AddRangeAsync(provinces, ct);
        await db.Districts.AddRangeAsync(districts, ct);
        await db.Wards.AddRangeAsync(wards, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedAdministrativeUnitsAsync(MasterDataDbContext db, CancellationToken ct)
    {
        if (await db.AdministrativeUnits.AnyAsync(ct)) return;

        var units = new[]
        {
            AdministrativeUnit.Create("01",    "Hà Nội",          "Hanoi",     1, null,  null,    true),
            AdministrativeUnit.Create("001",   "Quận Ba Đình",    "Ba Dinh",   2, "01",  null,    true),
            AdministrativeUnit.Create("00001", "Phường Phúc Xá",  "Phuc Xa",   3, "001", null,    true),
            AdministrativeUnit.Create("00002", "Phường Nguyễn Trung Trực (cũ)", "NTT Old", 3, "001", "00001", false,
                new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)),
        };

        await db.AdministrativeUnits.AddRangeAsync(units, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedCaseTypesAsync(MasterDataDbContext db, CancellationToken ct)
    {
        if (await db.CaseTypes.AnyAsync(ct)) return;

        var types = new[]
        {
            CaseType.Create("CMND_CCCD", "Cấp/đổi CMND/CCCD", "ID Card Issuance"),
            CaseType.Create("DKKD",      "Đăng ký kinh doanh", "Business Registration"),
            CaseType.Create("GPXD",      "Giấy phép xây dựng", "Construction Permit"),
        };

        await db.CaseTypes.AddRangeAsync(types, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedJobTitlesAsync(MasterDataDbContext db, CancellationToken ct)
    {
        if (await db.JobTitles.AnyAsync(ct)) return;

        var titles = new[]
        {
            JobTitle.Create("TRUONG_PHONG", "Trưởng phòng",  "Department Head"),
            JobTitle.Create("PHO_PHONG",    "Phó phòng",     "Deputy Head"),
            JobTitle.Create("CHUYEN_VIEN",  "Chuyên viên",   "Specialist"),
            JobTitle.Create("VAN_THU",      "Văn thư",       "Secretary"),
        };

        await db.JobTitles.AddRangeAsync(titles, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task LoadHotDataCacheAsync(
        IServiceProvider sp, MasterDataDbContext db, CancellationToken ct)
    {
        var cache = sp.GetRequiredService<IMemoryCache>();
        var opts = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove,
            Size = 1
        };

        var provinces = await db.Provinces.AsNoTracking().OrderBy(p => p.SortOrder).ToListAsync(ct);
        var districts = await db.Districts.AsNoTracking().OrderBy(d => d.SortOrder).ToListAsync(ct);
        var wards = await db.Wards.AsNoTracking().OrderBy(w => w.SortOrder).ToListAsync(ct);

        cache.Set("masterdata:provinces", provinces, opts);
        cache.Set("masterdata:districts", districts, opts);
        cache.Set("masterdata:wards", wards, opts);
    }
}
