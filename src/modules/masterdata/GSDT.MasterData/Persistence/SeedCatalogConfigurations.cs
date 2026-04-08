using GSDT.MasterData.Entities.Catalogs;

namespace GSDT.MasterData.Persistence;

/// <summary>
/// EF Core model configurations for the 14 system-wide seed catalogs.
/// Called from MasterDataDbContext.OnModelCreating.
/// Seed data uses SeedCatalogHelper.SeedId for deterministic, migration-safe Guids.
/// EF HasData receives anonymous objects to bypass private property setters.
/// </summary>
internal static class SeedCatalogConfigurations
{
    public static void Configure(ModelBuilder mb)
    {
        ConfigureIndustrySectors(mb);
        ConfigureProjectGroups(mb);
        ConfigureDomesticProjectStatuses(mb);
        ConfigureOdaProjectStatuses(mb);
        ConfigureAdjustmentContents(mb);
        ConfigureBidSelectionForms(mb);
        ConfigureBidSelectionMethods(mb);
        ConfigureContractForms(mb);
        ConfigureBidSectorTypes(mb);
        ConfigureEvaluationTypes(mb);
        ConfigureAuditConclusionTypes(mb);
        ConfigureViolationTypes(mb);
        ConfigureViolationActions(mb);
        ConfigureOdaProjectTypes(mb);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds an anonymous seed object compatible with EF HasData.
    /// Anonymous objects bypass private setters; EF maps by property name.
    /// CreatedAt / IsDeleted are required because Entity base class declares them.
    /// </summary>
    private static object Row(string table, int index, string code, string name, int sortOrder) =>
        new
        {
            Id         = SeedCatalogHelper.SeedId(table, index),
            Code       = code,
            Name       = name,
            IsActive   = true,
            SortOrder  = sortOrder,
            CreatedAt  = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt  = (DateTimeOffset?)null,
            IsDeleted  = false
        };

    private static void SimpleTable<T>(ModelBuilder mb, string table, object[] rows)
        where T : class
    {
        mb.Entity<T>(e =>
        {
            e.ToTable(table);
            e.HasKey("Id");
            e.Property<string>("Code").HasMaxLength(50).IsRequired();
            e.Property<string>("Name").HasMaxLength(200).IsRequired();
            e.HasIndex("Code").IsUnique();
            e.HasData(rows);
        });
    }

    // ── 14 seed catalogs ──────────────────────────────────────────────────────

    private static void ConfigureIndustrySectors(ModelBuilder mb)
    {
        mb.Entity<IndustrySector>(e =>
        {
            e.ToTable("IndustrySectors");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("IndustrySectors", 1,  "GT",  "Giao thong",  1),
                Row("IndustrySectors", 2,  "TL",  "Thuy loi",    2),
                Row("IndustrySectors", 3,  "NN",  "Nong nghiep", 3),
                Row("IndustrySectors", 4,  "YT",  "Y te",        4),
                Row("IndustrySectors", 5,  "GD",  "Giao duc",    5),
                Row("IndustrySectors", 6,  "KHCN","Khoa hoc CN", 6),
                Row("IndustrySectors", 7,  "VH",  "Van hoa",     7),
                Row("IndustrySectors", 8,  "TDTT","The duc TT",  8),
                Row("IndustrySectors", 9,  "MT",  "Moi truong",  9),
                Row("IndustrySectors", 10, "CNTT","CNTT",        10),
                Row("IndustrySectors", 11, "QP",  "Quoc phong",  11),
                Row("IndustrySectors", 12, "AN",  "An ninh",     12),
                Row("IndustrySectors", 13, "KC",  "Khac",        13)
            );
        });
    }

    private static void ConfigureProjectGroups(ModelBuilder mb)
    {
        mb.Entity<ProjectGroup>(e =>
        {
            e.ToTable("ProjectGroups");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("ProjectGroups", 1, "QG", "Quan trong quoc gia", 1),
                Row("ProjectGroups", 2, "A",  "Nhom A",              2),
                Row("ProjectGroups", 3, "B",  "Nhom B",              3),
                Row("ProjectGroups", 4, "C",  "Nhom C",              4)
            );
        });
    }

    private static void ConfigureDomesticProjectStatuses(ModelBuilder mb)
    {
        mb.Entity<DomesticProjectStatus>(e =>
        {
            e.ToTable("DomesticProjectStatuses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("DomesticProjectStatuses", 1, "CBDT", "Chuan bi dau tu", 1),
                Row("DomesticProjectStatuses", 2, "TH",   "Thuc hien",       2),
                Row("DomesticProjectStatuses", 3, "HT",   "Hoan thanh",      3),
                Row("DomesticProjectStatuses", 4, "TD",   "Tam dung",        4),
                Row("DomesticProjectStatuses", 5, "HB",   "Huy bo",          5)
            );
        });
    }

    private static void ConfigureOdaProjectStatuses(ModelBuilder mb)
    {
        mb.Entity<OdaProjectStatus>(e =>
        {
            e.ToTable("OdaProjectStatuses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("OdaProjectStatuses", 1,  "CB",  "Chuan bi",   1),
                Row("OdaProjectStatuses", 2,  "DN",  "Dam phan",   2),
                Row("OdaProjectStatuses", 3,  "KK",  "Ky ket",     3),
                Row("OdaProjectStatuses", 4,  "HL",  "Hieu luc",   4),
                Row("OdaProjectStatuses", 5,  "TH",  "Thuc hien",  5),
                Row("OdaProjectStatuses", 6,  "GH",  "Gia han",    6),
                Row("OdaProjectStatuses", 7,  "DC",  "Dieu chinh", 7),
                Row("OdaProjectStatuses", 8,  "DG",  "Dong cua",   8),
                Row("OdaProjectStatuses", 9,  "HTA", "Hoan thanh", 9),
                Row("OdaProjectStatuses", 10, "TL",  "Thanh ly",   10),
                Row("OdaProjectStatuses", 11, "TDA", "Tam dung",   11),
                Row("OdaProjectStatuses", 12, "HBA", "Huy bo",     12)
            );
        });
    }

    private static void ConfigureAdjustmentContents(ModelBuilder mb)
    {
        mb.Entity<AdjustmentContent>(e =>
        {
            e.ToTable("AdjustmentContents");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("AdjustmentContents", 1, "TMDT", "Tong muc dau tu", 1),
                Row("AdjustmentContents", 2, "TDO",  "Tien do",         2),
                Row("AdjustmentContents", 3, "QM",   "Quy mo",          3),
                Row("AdjustmentContents", 4, "CCV",  "Co cau von",      4),
                Row("AdjustmentContents", 5, "CDT",  "Chu dau tu",      5),
                Row("AdjustmentContents", 6, "BQL",  "Ban QLDA",        6),
                Row("AdjustmentContents", 7, "KC",   "Khac",            7)
            );
        });
    }

    private static void ConfigureBidSelectionForms(ModelBuilder mb)
    {
        mb.Entity<BidSelectionForm>(e =>
        {
            e.ToTable("BidSelectionForms");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("BidSelectionForms", 1, "DTR",   "Dau thau rong rai",       1),
                Row("BidSelectionForms", 2, "DTH",   "Dau thau han che",        2),
                Row("BidSelectionForms", 3, "CDT",   "Chi dinh thau",           3),
                Row("BidSelectionForms", 4, "CHCC",  "Chao hang canh tranh",    4),
                Row("BidSelectionForms", 5, "MSTT",  "Mua sam truc tiep",       5),
                Row("BidSelectionForms", 6, "TTH",   "Tu thuc hien",            6),
                Row("BidSelectionForms", 7, "LNTDB", "Lua chon nha thau dac biet", 7),
                Row("BidSelectionForms", 8, "TGCD",  "Tham gia cong dong",      8),
                Row("BidSelectionForms", 9, "LNTCN", "Lua chon tu van ca nhan", 9)
            );
        });
    }

    private static void ConfigureBidSelectionMethods(ModelBuilder mb)
    {
        mb.Entity<BidSelectionMethod>(e =>
        {
            e.ToTable("BidSelectionMethods");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("BidSelectionMethods", 1, "1G1T", "1GD-1THS", 1),
                Row("BidSelectionMethods", 2, "1G2T", "1GD-2THS", 2),
                Row("BidSelectionMethods", 3, "2G1T", "2GD-1THS", 3),
                Row("BidSelectionMethods", 4, "2G2T", "2GD-2THS", 4)
            );
        });
    }

    private static void ConfigureContractForms(ModelBuilder mb)
    {
        mb.Entity<ContractForm>(e =>
        {
            e.ToTable("ContractForms");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("ContractForms", 1, "TG",   "Tron goi",            1),
                Row("ContractForms", 2, "DGCD", "Don gia co dinh",     2),
                Row("ContractForms", 3, "DGDC", "Don gia dieu chinh",  3),
                Row("ContractForms", 4, "TGI",  "Theo thoi gian",      4),
                Row("ContractForms", 5, "TLPP", "Theo ty le phan tram",5),
                Row("ContractForms", 6, "HH",   "Hon hop",             6),
                Row("ContractForms", 7, "CSRR", "Chia se rui ro",      7),
                Row("ContractForms", 8, "HTC",  "Hop tac",             8),
                Row("ContractForms", 9, "KC",   "Khac",                9)
            );
        });
    }

    private static void ConfigureBidSectorTypes(ModelBuilder mb)
    {
        mb.Entity<BidSectorType>(e =>
        {
            e.ToTable("BidSectorTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("BidSectorTypes", 1, "XL",   "Xay lap",          1),
                Row("BidSectorTypes", 2, "MMHH", "Mua sam hang hoa", 2),
                Row("BidSectorTypes", 3, "TV",   "Tu van",           3),
                Row("BidSectorTypes", 4, "PTV",  "Phi tu van",       4),
                Row("BidSectorTypes", 5, "HH",   "Hon hop",          5)
            );
        });
    }

    private static void ConfigureEvaluationTypes(ModelBuilder mb)
    {
        mb.Entity<EvaluationType>(e =>
        {
            e.ToTable("EvaluationTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("EvaluationTypes", 1, "DK",  "Dinh ky",  1),
                Row("EvaluationTypes", 2, "GK",  "Giua ky",  2),
                Row("EvaluationTypes", 3, "KT",  "Ket thuc", 3),
                Row("EvaluationTypes", 4, "DX",  "Dot xuat", 4),
                Row("EvaluationTypes", 5, "TDD", "Tac dong", 5)
            );
        });
    }

    private static void ConfigureAuditConclusionTypes(ModelBuilder mb)
    {
        mb.Entity<AuditConclusionType>(e =>
        {
            e.ToTable("AuditConclusionTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("AuditConclusionTypes", 1, "CN",   "Chap nhan",            1),
                Row("AuditConclusionTypes", 2, "CNNT", "Chap nhan co ngoai tru", 2),
                Row("AuditConclusionTypes", 3, "TC",   "Tu choi",              3)
            );
        });
    }

    private static void ConfigureViolationTypes(ModelBuilder mb)
    {
        mb.Entity<ViolationType>(e =>
        {
            e.ToTable("ViolationTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("ViolationTypes", 1, "TD", "Tien do",   1),
                Row("ViolationTypes", 2, "CL", "Chat luong", 2),
                Row("ViolationTypes", 3, "TC", "Tai chinh",  3),
                Row("ViolationTypes", 4, "KC", "Khac",       4)
            );
        });
    }

    private static void ConfigureViolationActions(ModelBuilder mb)
    {
        mb.Entity<ViolationAction>(e =>
        {
            e.ToTable("ViolationActions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("ViolationActions", 1, "XPHC",  "Xu phat hanh chinh",      1),
                Row("ViolationActions", 2, "XLKL",  "Xu ly ky luat",           2),
                Row("ViolationActions", 3, "CCQDT", "Chuyen co quan dieu tra", 3)
            );
        });
    }

    private static void ConfigureOdaProjectTypes(ModelBuilder mb)
    {
        mb.Entity<OdaProjectType>(e =>
        {
            e.ToTable("OdaProjectTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasData(
                Row("OdaProjectTypes", 1, "HTKT", "Ho tro ky thuat",   1),
                Row("OdaProjectTypes", 2, "DADT", "Du an dau tu",      2),
                Row("OdaProjectTypes", 3, "TANS", "Tai tro ngan sach", 3)
            );
        });
    }
}
