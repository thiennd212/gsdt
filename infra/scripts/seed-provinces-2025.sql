-- Seed 34 tỉnh/thành phố Việt Nam sau sáp nhập (QĐ 19/2025/QĐ-TTg, hiệu lực 1/7/2025)
-- Mô hình 2 cấp: Tỉnh → Xã (bỏ cấp huyện)
-- Source: https://thuvienphapluat.vn/chinh-sach-phap-luat-moi/vn/ho-tro-phap-luat/chinh-sach-moi/89476
-- Idempotent: clears old data and re-inserts all 34 records.

-- Clear old province data (cascade will be handled by FK if any)
DELETE FROM masterdata.Wards;
DELETE FROM masterdata.Districts;
DELETE FROM masterdata.Provinces;

DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();

-- 34 tỉnh/thành phố theo QĐ 19/2025/QĐ-TTg
INSERT INTO masterdata.Provinces (Id, Code, NameVi, NameEn, IsActive, SortOrder, CreatedAt, IsDeleted) VALUES
-- 6 thành phố trực thuộc trung ương
(NEWID(), '01', N'Thành phố Hà Nội',           'Ha Noi City',              1, 1,  @now, 0),
(NEWID(), '31', N'Thành phố Hải Phòng',        'Hai Phong City',           1, 2,  @now, 0),
(NEWID(), '46', N'Thành phố Huế',              'Hue City',                 1, 3,  @now, 0),
(NEWID(), '48', N'Thành phố Đà Nẵng',          'Da Nang City',             1, 4,  @now, 0),
(NEWID(), '79', N'Thành phố Hồ Chí Minh',      'Ho Chi Minh City',         1, 5,  @now, 0),
(NEWID(), '92', N'Thành phố Cần Thơ',          'Can Tho City',             1, 6,  @now, 0),
-- 28 tỉnh (theo thứ tự mã)
(NEWID(), '04', N'Tỉnh Cao Bằng',              'Cao Bang Province',        1, 7,  @now, 0),
(NEWID(), '08', N'Tỉnh Tuyên Quang',           'Tuyen Quang Province',     1, 8,  @now, 0),
(NEWID(), '11', N'Tỉnh Điện Biên',             'Dien Bien Province',       1, 9,  @now, 0),
(NEWID(), '12', N'Tỉnh Lai Châu',              'Lai Chau Province',        1, 10, @now, 0),
(NEWID(), '14', N'Tỉnh Sơn La',                'Son La Province',          1, 11, @now, 0),
(NEWID(), '15', N'Tỉnh Lào Cai',               'Lao Cai Province',         1, 12, @now, 0),
(NEWID(), '19', N'Tỉnh Thái Nguyên',           'Thai Nguyen Province',     1, 13, @now, 0),
(NEWID(), '20', N'Tỉnh Lạng Sơn',              'Lang Son Province',        1, 14, @now, 0),
(NEWID(), '22', N'Tỉnh Quảng Ninh',            'Quang Ninh Province',      1, 15, @now, 0),
(NEWID(), '24', N'Tỉnh Bắc Ninh',              'Bac Ninh Province',        1, 16, @now, 0),
(NEWID(), '25', N'Tỉnh Phú Thọ',               'Phu Tho Province',         1, 17, @now, 0),
(NEWID(), '33', N'Tỉnh Hưng Yên',              'Hung Yen Province',        1, 18, @now, 0),
(NEWID(), '37', N'Tỉnh Ninh Bình',             'Ninh Binh Province',       1, 19, @now, 0),
(NEWID(), '38', N'Tỉnh Thanh Hóa',             'Thanh Hoa Province',       1, 20, @now, 0),
(NEWID(), '40', N'Tỉnh Nghệ An',               'Nghe An Province',         1, 21, @now, 0),
(NEWID(), '42', N'Tỉnh Hà Tĩnh',               'Ha Tinh Province',         1, 22, @now, 0),
(NEWID(), '44', N'Tỉnh Quảng Trị',             'Quang Tri Province',       1, 23, @now, 0),
(NEWID(), '51', N'Tỉnh Quảng Ngãi',            'Quang Ngai Province',      1, 24, @now, 0),
(NEWID(), '52', N'Tỉnh Gia Lai',               'Gia Lai Province',         1, 25, @now, 0),
(NEWID(), '56', N'Tỉnh Khánh Hòa',             'Khanh Hoa Province',       1, 26, @now, 0),
(NEWID(), '66', N'Tỉnh Đắk Lắk',              'Dak Lak Province',         1, 27, @now, 0),
(NEWID(), '68', N'Tỉnh Lâm Đồng',              'Lam Dong Province',        1, 28, @now, 0),
(NEWID(), '75', N'Tỉnh Đồng Nai',              'Dong Nai Province',        1, 29, @now, 0),
(NEWID(), '80', N'Tỉnh Tây Ninh',              'Tay Ninh Province',        1, 30, @now, 0),
(NEWID(), '82', N'Tỉnh Đồng Tháp',             'Dong Thap Province',       1, 31, @now, 0),
(NEWID(), '86', N'Tỉnh Vĩnh Long',             'Vinh Long Province',       1, 32, @now, 0),
(NEWID(), '91', N'Tỉnh An Giang',              'An Giang Province',        1, 33, @now, 0),
(NEWID(), '96', N'Tỉnh Cà Mau',                'Ca Mau Province',          1, 34, @now, 0);

PRINT 'Seeded 34 provinces/cities per QD 19/2025/QD-TTg (effective 1/7/2025)';
PRINT 'Note: District level eliminated in new 2-tier model (Province -> Commune)';
PRINT 'Districts table cleared. Wards/communes to be seeded separately (3321 units).';
