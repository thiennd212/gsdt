-- Seed all masterdata catalogs for development environment.
-- Idempotent: skips tables that already have data.
-- TenantId: 00000000-0000-0000-0000-000000000001 (default dev tenant)

DECLARE @tid UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000001';
DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();
DECLARE @sys UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000'; -- system user for CreatedBy

-- 1. ManagingAuthorities
IF NOT EXISTS (SELECT 1 FROM masterdata.ManagingAuthorities)
INSERT INTO masterdata.ManagingAuthorities (Id, TenantId, Code, Name, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'BTC',    N'Bộ Tài chính',              1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BKHDT',  N'Bộ Kế hoạch và Đầu tư',     1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BGTVT',  N'Bộ Giao thông Vận tải',      1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BXD',    N'Bộ Xây dựng',                1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BNNPT',  N'Bộ Nông nghiệp & PTNT',      1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BYT',    N'Bộ Y tế',                    1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BGDDT',  N'Bộ Giáo dục & Đào tạo',      1, @now, 0, @sys, 0),
(NEWID(), @tid, 'UBNDTP', N'UBND Thành phố',             1, @now, 0, @sys, 0);

-- 2. ProjectOwners
IF NOT EXISTS (SELECT 1 FROM masterdata.ProjectOwners)
INSERT INTO masterdata.ProjectOwners (Id, TenantId, Code, Name, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'CDT01', N'Ban QLDA Đầu tư XD công trình giao thông', 1, @now, 0, @sys, 0),
(NEWID(), @tid, 'CDT02', N'Ban QLDA Đầu tư XD công trình dân dụng',   1, @now, 0, @sys, 0),
(NEWID(), @tid, 'CDT03', N'Ban QLDA Đầu tư XD công trình NN&PTNT',    1, @now, 0, @sys, 0),
(NEWID(), @tid, 'CDT04', N'Sở Kế hoạch và Đầu tư',                    1, @now, 0, @sys, 0),
(NEWID(), @tid, 'CDT05', N'Sở Giao thông Vận tải',                     1, @now, 0, @sys, 0),
(NEWID(), @tid, 'CDT06', N'Sở Xây dựng',                               1, @now, 0, @sys, 0);

-- 3. NationalTargetPrograms
IF NOT EXISTS (SELECT 1 FROM masterdata.NationalTargetPrograms)
INSERT INTO masterdata.NationalTargetPrograms (Id, TenantId, Code, Name, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'CTMTQG01', N'CT MTQG Xây dựng Nông thôn mới',             1, @now, 0, @sys, 0),
(NEWID(), @tid, 'CTMTQG02', N'CT MTQG Giảm nghèo bền vững',                 1, @now, 0, @sys, 0),
(NEWID(), @tid, 'CTMTQG03', N'CT MTQG phát triển KT-XH vùng DTTS&MN',      1, @now, 0, @sys, 0);

-- 4. ProjectManagementUnits
IF NOT EXISTS (SELECT 1 FROM masterdata.ProjectManagementUnits)
INSERT INTO masterdata.ProjectManagementUnits (Id, TenantId, Code, Name, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'PMU01', N'Ban QLDA Đầu tư XD khu vực Bắc',   1, @now, 0, @sys, 0),
(NEWID(), @tid, 'PMU02', N'Ban QLDA Đầu tư XD khu vực Trung', 1, @now, 0, @sys, 0),
(NEWID(), @tid, 'PMU03', N'Ban QLDA Đầu tư XD khu vực Nam',   1, @now, 0, @sys, 0),
(NEWID(), @tid, 'PMU04', N'Ban QLDA chuyên ngành GTVT',        1, @now, 0, @sys, 0);

-- 5. InvestmentDecisionAuthorities
IF NOT EXISTS (SELECT 1 FROM masterdata.InvestmentDecisionAuthorities)
INSERT INTO masterdata.InvestmentDecisionAuthorities (Id, TenantId, Code, Name, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'QH',      N'Quốc hội',                       1, @now, 0, @sys, 0),
(NEWID(), @tid, 'CP',      N'Chính phủ / Thủ tướng CP',       1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BTRUONG', N'Bộ trưởng / Thủ trưởng CQ',     1, @now, 0, @sys, 0),
(NEWID(), @tid, 'UBND',    N'UBND cấp tỉnh',                  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'HDND',    N'HĐND cấp tỉnh',                  1, @now, 0, @sys, 0);

-- 6. Contractors
IF NOT EXISTS (SELECT 1 FROM masterdata.Contractors)
INSERT INTO masterdata.Contractors (Id, TenantId, Code, Name, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'NT01', N'Tổng Công ty XD Trường Sơn',        1, @now, 0, @sys, 0),
(NEWID(), @tid, 'NT02', N'Tổng Công ty XD Sông Đà',           1, @now, 0, @sys, 0),
(NEWID(), @tid, 'NT03', N'Công ty CP XD & TM Đại Phát',       1, @now, 0, @sys, 0),
(NEWID(), @tid, 'NT04', N'Công ty TNHH Xây dựng Hòa Bình',   1, @now, 0, @sys, 0),
(NEWID(), @tid, 'NT05', N'Liên danh CIENCO4 - VINACONEX',     1, @now, 0, @sys, 0);

-- 7. DocumentTypes
IF NOT EXISTS (SELECT 1 FROM masterdata.DocumentTypes)
INSERT INTO masterdata.DocumentTypes (Id, TenantId, Code, Name, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'QD', N'Quyết định',   1, @now, 0, @sys, 0),
(NEWID(), @tid, 'CV', N'Công văn',      1, @now, 0, @sys, 0),
(NEWID(), @tid, 'TB', N'Thông báo',     1, @now, 0, @sys, 0),
(NEWID(), @tid, 'HD', N'Hợp đồng',      1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BC', N'Báo cáo',       1, @now, 0, @sys, 0),
(NEWID(), @tid, 'TT', N'Tờ trình',      1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BB', N'Biên bản',      1, @now, 0, @sys, 0);

-- 8. ProjectImplementationStatuses
IF NOT EXISTS (SELECT 1 FROM masterdata.ProjectImplementationStatuses)
INSERT INTO masterdata.ProjectImplementationStatuses (Id, TenantId, Code, Name, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'CB', N'Chuẩn bị đầu tư', 1, @now, 0, @sys, 0),
(NEWID(), @tid, 'TH', N'Thực hiện dự án',  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'KT', N'Kết thúc đầu tư',  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'VH', N'Vận hành',          1, @now, 0, @sys, 0),
(NEWID(), @tid, 'TD', N'Tạm dừng',          1, @now, 0, @sys, 0);

-- 9. Banks
IF NOT EXISTS (SELECT 1 FROM masterdata.Banks)
INSERT INTO masterdata.Banks (Id, TenantId, Code, Name, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'NHNN', N'Ngân hàng Nhà nước Việt Nam',                1, @now, 0, @sys, 0),
(NEWID(), @tid, 'VCB',  N'Ngân hàng TMCP Ngoại thương (Vietcombank)',  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'CTG',  N'Ngân hàng TMCP Công Thương (VietinBank)',    1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BIDV', N'Ngân hàng TMCP Đầu tư & PT (BIDV)',         1, @now, 0, @sys, 0),
(NEWID(), @tid, 'AGR',  N'Ngân hàng NN & PTNT (Agribank)',             1, @now, 0, @sys, 0),
(NEWID(), @tid, 'VDB',  N'Ngân hàng Phát triển Việt Nam (VDB)',        1, @now, 0, @sys, 0);

-- 10. ManagingAgencies
IF NOT EXISTS (SELECT 1 FROM masterdata.ManagingAgencies)
INSERT INTO masterdata.ManagingAgencies (Id, TenantId, Code, Name, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'BKHDT', N'Bộ Kế hoạch và Đầu tư', 1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BTC',   N'Bộ Tài chính',            1, @now, 0, @sys, 0),
(NEWID(), @tid, 'KTNN',  N'Kiểm toán Nhà nước',      1, @now, 0, @sys, 0),
(NEWID(), @tid, 'TTCP',  N'Thanh tra Chính phủ',      1, @now, 0, @sys, 0);

-- 11. GovernmentAgencies (tenant-scoped, hierarchical)
IF NOT EXISTS (SELECT 1 FROM masterdata.GovernmentAgencies)
INSERT INTO masterdata.GovernmentAgencies (Id, TenantId, Code, Name, AgencyType, SortOrder, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'CP',    N'Chính phủ',                  'GOV', 1,  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'QH',    N'Quốc hội',                   'GOV', 2,  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BTC',   N'Bộ Tài chính',               'MIN', 3,  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BKHDT', N'Bộ Kế hoạch và Đầu tư',     'MIN', 4,  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BGTVT', N'Bộ Giao thông Vận tải',      'MIN', 5,  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BXD',   N'Bộ Xây dựng',               'MIN', 6,  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BNNPT', N'Bộ Nông nghiệp & PTNT',     'MIN', 7,  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'BYT',   N'Bộ Y tế',                   'MIN', 8,  1, @now, 0, @sys, 0);

-- 12. Investors (tenant-scoped)
IF NOT EXISTS (SELECT 1 FROM masterdata.Investors)
INSERT INTO masterdata.Investors (Id, TenantId, InvestorType, BusinessIdOrCccd, NameVi, NameEn, IsActive, CreatedAt, IsDeleted, CreatedBy, ClassificationLevel) VALUES
(NEWID(), @tid, 'SOE', '0100100079', N'Tập đoàn Điện lực Việt Nam (EVN)',           'EVN',              1, @now, 0, @sys, 0),
(NEWID(), @tid, 'SOE', '0100107866', N'Tập đoàn Dầu khí Việt Nam (PVN)',            'PetroVietnam',     1, @now, 0, @sys, 0),
(NEWID(), @tid, 'SOE', '0100105052', N'Tổng Công ty Đường sắt Việt Nam (VNR)',      'Vietnam Railways',  1, @now, 0, @sys, 0),
(NEWID(), @tid, 'SOE', '0100109106', N'Tập đoàn Viễn thông Quân đội (Viettel)',     'Viettel Group',    1, @now, 0, @sys, 0),
(NEWID(), @tid, 'GOV', '0106005319', N'Công ty TNHH MTV PT Khu CNC Hòa Lạc',       'HHTP Dev Co',      1, @now, 0, @sys, 0);

PRINT 'Masterdata seeding complete — 12 tables seeded';
