# SRS v1.2 Analysis — Delta from v1.1 (Deep-dive)

**Date:** 2026-04-08 | **Source:** `03426.HT GSDT_SRS_1.2.docx` (3.3MB, 251 paragraphs, 31 tables, 69 screenshots)
**Baseline:** SRS v1.1 (Phase 1 — all 10 phases DONE, merged to main)
**Analyst:** Claude | **Reviewer:** thiennd212

---

## 1. Executive Summary

SRS v1.2 expands from **2 project types** (Domestic + ODA) to **6 project types**, adding 4 new categories with 81 new functions. Detailed field analysis reveals ~60% reuse from Phase 1 (shared popups, tabs, and entities).

| Metric | v1.1 (done) | v1.2 (new) | Total |
|--------|-------------|------------|-------|
| Project types | 2 | +4 | 6 |
| Functions | 46 | +81 | 127 |
| Field spec tables | 12 | +19 | 31 |
| Field spec rows | ~400 | ~450 | ~850 |
| Screenshots | ~30 | ~39 | 69 |
| Estimated effort | — | 29-39d | — |

---

## 2. Project Type Taxonomy

```
GSDT
├── ĐTC (Đầu tư công) ← Phase 1 DONE
│   ├── 5.1 Dự án trong nước (23 fn, 6 tabs, T5-T12) ✓
│   └── 5.2 Dự án ODA (23 fn, 6 tabs, T13-T16) ✓
├── 5.3 Dự án theo luật PPP ← NEW (23 fn, 7 tabs, T17-T23)
└── 5.4 Dự án theo luật đầu tư ← NEW
    ├── Dự án DNNN (22 fn, 6 tabs, T24-T25)
    ├── Dự án NĐT trong nước (18 fn, ~6 tabs, no field table — "tương tự DNNN")
    └── Dự án FDI (18 fn, ~6 tabs, no field table — "tương tự NĐT")
```

---

## 3. TN/ODA v1.2 vs v1.1 — Change Detection

**Result: NO FIELD-LEVEL CHANGES detected.** The v1.2 document reproduces the exact same field specs for sections 5.1 (Tables 5-12) and 5.2 (Tables 13-16). Existing Phase 1 implementation remains 100% valid.

Note: "Lịch sử sửa đổi" table shows only "1.0" entry dated 03/4/2026 with empty change description — v1.0→v1.2 changelog is not documented in the SRS.

---

## 4. Shared Component Analysis (Field-level verified)

### 4.1 IDENTICAL Patterns (copy-paste from TN)

| Component | Where shared | Fields | Proof |
|-----------|-------------|--------|-------|
| **Gói thầu popup** | TN(T6), PPP(T19), DNNN(implicit) | 20+ fields: KHLCNT, tên, checkbox kỹ thuật/giám sát, hạng mục table, hình thức/phương thức/thời gian lựa chọn, hình thức HĐ, kết quả (đơn vị trúng thầu, giá, QĐ, ghi chú) | T6.R19-R46 ≡ T19.R15-R42 |
| **Hợp đồng popup** | TN(T6), PPP(T19) | 10 fields: KHLCNT, gói thầu, tên HĐ, số HĐ, hình thức, ngày ký, giá trị, hiệu lực, kết thúc, file | T6.R51-R62 ≡ T19.R47-R58 |
| **Tài liệu dự án tab** | ALL 6 types | 14 fields: tên VB, ngày ban hành, số ký hiệu, loại VB, trích yếu, tìm kiếm, thao tác | SRS explicitly states "tương tự" for PPP/DNNN/NĐT/FDI |
| **Thiết kế thi công popup** | PPP(T17), DNNN(T24) | 15+ fields: QĐ phê duyệt, thông tin chung, dự toán 7 hạng mục (thiết bị, xây lắp, GPMB, QLDA, tư vấn, dự phòng, khác) + auto-sum + hạng mục table | T17.R41-R67 ≡ T24.R54-R81 |

### 4.2 SIMILAR Patterns (minor variations)

| Component | Variation | Impact |
|-----------|-----------|--------|
| **Thanh tra/KT/KiemToán tab** | TN(T8) has 3 left-menu sub-sections with inline Lịch sử bản ghi. PPP(T21) restructures into Kiểm tra + Đánh giá + Kết luận with separate Danh sách tables. Core fields identical but UI layout differs. | Need separate FE component but shared BE entities |
| **Khai thác/vận hành tab** | TN(T9): 10 fields — checkboxes (quyết toán/nghiệm thu/khai thác), tiến độ, chủ sử dụng. PPP(T22): 17 fields — adds **revenue reporting** (năm/kỳ, doanh thu, lũy kế, chia sẻ tăng/giảm) + different checkboxes. DNNN: SRS says "tương tự PPP". | PPP tab is a **superset** of TN tab. Can build PPP version and conditionally hide revenue fields for TN. |
| **Kế hoạch vốn** | TN(T6): "Kế hoạch giao vốn" with Lần cấp vốn (1-20), Tổng số vốn. PPP(T19): "Kế hoạch bố trí vốn NN" with Số vốn NN theo QĐ. DNNN: none (no capital plan tab). | Same CapitalPlan entity, different label + 1-2 field variations |
| **Giải ngân tab** | TN(T7): Vốn ĐTC (giải ngân tháng + tháng trước + lũy kế) + Vốn khác. PPP(T20): Vốn nhà nước + Vốn CSH + Vốn vay (each with kỳ + lũy kế). | PPP has 3 capital sources vs TN's 2. Different entity columns needed. |
| **Danh sách/Tìm kiếm** | TN(T11): 7 search filters (tên, CQQL, CĐT, nhóm, tình trạng, DA thành phần, tìm kiếm). PPP(T23): 8 filters (+Địa điểm, Lĩnh vực đầu tư, -CQQL→CQCQ). DNNN(T25): 5 filters (tên, CQCQ, NĐT, tình trạng, địa điểm). | Shared list component with configurable filter set. |

### 4.3 UNIQUE to Each Type (must build new)

| Project Type | Unique Component | Fields | Notes |
|--------------|-----------------|--------|-------|
| **PPP** | Tab2: Hợp đồng dự án PPP | 30 rows — Lựa chọn NĐT (hình thức, NĐT multiselect, QĐ), TMĐT breakdown (Vốn NN: NSTW+NSĐP+NSNN khác, Vốn CSH + tỷ lệ, Vốn vay), Tiến độ/Thời hạn, HĐ ký kết (cơ quan, số, ngày, khởi công, hoàn thành), Cơ chế chia sẻ doanh thu | Entirely new tab — no P1 equivalent |
| **PPP** | Tab1: Loại hợp đồng PPP | Radio: BOT / BT (→ BT đất/tiền/không TT) / Các loại khác (→ BTO/BOO/O&M/BTL/BLT/hỗn hợp) | Conditional combobox based on radio selection |
| **PPP** | Tab6: Revenue reporting | Năm (current-5..current), Kỳ (6 tháng/1 năm), Doanh thu kỳ, Lũy kế, Chia sẻ tăng/giảm, Khó khăn, Kiến nghị | New sub-module, periodic reporting pattern |
| **DNNN** | Tab1: GCNĐKĐT section | Số GCN, Ngày cấp, File, Vốn đầu tư, Vốn CSH, Tỷ lệ CSH | Inline section with history list |
| **DNNN** | Tab1: Capital structure | Vốn CSH + Vốn vay ODA + Vốn vay TCTD (vs TN's NSTW+NSĐP+ĐTC khác) | Different column set for InvestmentDecision |
| **DNNN** | Tab1: KKT/KCN field | Địa điểm adds "Tên KKT, KCN, KCX, FTZ, TTTC" field with tooltip | 1 additional field in ProjectLocation |
| **DNNN** | Hợp đồng lựa chọn NĐT | SRS lists this as separate function (R78) but no dedicated field table | May be a simple form or reference to PPP Tab2 NĐT section |

---

## 5. Detailed Tab Mapping — All 6 Project Types

### Tab Structure Comparison

| Tab # | TN (P1 done) | ODA (P1 done) | PPP (new) | DNNN (new) | NĐT (new) | FDI (new) |
|-------|-------------|---------------|-----------|-----------|-----------|----------|
| 1 | Thông tin chung (53 fields) | Thông tin chung (83 fields) | Thông tin QĐĐT (71 fields) | Thông tin QĐĐT (84 fields) | "tương tự DNNN" | "tương tự NĐT" |
| 2 | THTH (68 fields) | THTH (72 fields) | HĐ dự án PPP (30 fields) ★ | Hợp đồng NĐT (?) | — | — |
| 3 | Giải ngân (14 fields) | Giải ngân (34 fields) | THTH (68 fields) | THTH (implicit) | THTH | THTH |
| 4 | Thanh tra (30 fields) | Thanh tra (shared) | Giải ngân (14 fields) | Thanh tra (shared) | Thanh tra | Thanh tra |
| 5 | Khai thác (10 fields) | Khai thác (shared) | Thanh tra (25 fields) | Khai thác (shared) | Khai thác | Khai thác |
| 6 | Tài liệu (14 fields) | Tài liệu (shared) | Khai thác+Revenue (17 fields) ★ | Tài liệu (shared) | Tài liệu | Tài liệu |
| 7 | — | — | Tài liệu (shared) | — | — | — |

★ = Entirely new, no P1 equivalent

### PPP-specific Business Rules (from field constraints)

1. **Loại hợp đồng** cascading: BOT → direct / BT → sub-combo (đất/tiền/không TT) / Khác → sub-combo (BTO/BOO/O&M/BTL/BLT/hỗn hợp)
2. **TMĐT validation**: Tổng == Vốn NN + CSH + Vay & huy động (enforced on save)
3. **Tỷ lệ CSH**: auto-calc = Vốn CSH / TMĐT, rounded to 2 decimals
4. **Vốn NN breakdown**: Tổng vốn NN = NSTW + NSĐP + NSNN khác (auto-calc, disabled field)
5. **Revenue lũy kế**: auto-sum of all "Doanh thu trong kỳ" records by selected reporting period
6. **Chia sẻ tăng/giảm doanh thu**: free-text (mechanism description, not computed)
7. **Đơn vị chuẩn bị**: "NĐT đề xuất" vs "Cơ quan thẩm quyền lập" (affects workflow?)

### DNNN-specific Business Rules

1. **Tỷ lệ vốn NN nắm giữ**: Percentage field, informational (no validation)
2. **Capital TMĐT**: Vốn CSH + Vốn vay ODA + Vốn vay TCTD (different from TN and PPP)
3. **GCNĐKĐT**: Optional section, multiple records (list with Sửa/Xóa)
4. **KKT/KCN/KCX/FTZ/TTTC**: Extra location field specific to industrial zones
5. **"Nghĩa vụ tài chính với ngân sách NN"**: Listed as function R80 but no field table — likely a sub-section of THTH tab

---

## 6. New Entity Model (Refined)

```
InvestmentProject (existing TPT base)
├── DomesticProject ✓ (done)
├── OdaProject ✓ (done)
├── PppProject ← NEW child TPT
│   ├── ContractType (BOT/BT/BTO/BOO/O&M/BTL/BLT/hỗn hợp)
│   ├── InvestorSelectionMethod (đấu thầu/đàm phán/chỉ định/khác)
│   ├── ProjectDuration (thời hạn HĐ, cơ chế chia sẻ)
│   └── ContractSigningInfo (cơ quan, số HĐ, ngày, khởi công, hoàn thành)
├── DnnnProject ← NEW child TPT
│   ├── StateOwnershipRatio (tỷ lệ vốn NN)
│   └── IndustrialZoneInfo (KKT/KCN/KCX/FTZ/TTTC)
├── NdtProject ← NEW child TPT (thin, mostly inherits DNNN)
└── FdiProject ← NEW child TPT (thin, inherits NĐT)

# Shared entities (Phase 1 - extend)
├── InvestmentDecision[] — ADD: PPP columns (Vốn NN/CSH/Vay), DNNN columns (CSH/ODA/TCTD)
├── CapitalPlan[] — ADD: PPP variant "Vốn NN tham gia" (vs TN "Kế hoạch giao vốn")
├── DisbursementRecord[] — ADD: PPP 3-source (Vốn NN/CSH/Vay) vs TN 2-source (ĐTC/Khác)
├── ProjectLocation[] — ADD: DNNN KKT/KCN field
├── DesignEstimate[] — NEW shared between PPP & DNNN (TKTT popup, 7 cost items + hạng mục)
├── BidPackage[] ✓ (identical popup)
├── Contract[] ✓ (identical popup)
├── InspectionRecord[] ✓ (shared but PPP has slightly different layout)
├── EvaluationRecord[] ✓
├── AuditRecord[] ✓ 
├── ViolationRecord[] ✓ 
├── ProjectDocument[] ✓ (identical)
└── OperationInfo ✓ — EXTEND for PPP: add RevenueReport[] child

# NEW entities
├── RegistrationCertificate[] — DNNN/NĐT/FDI (GCNĐKĐT: số, ngày cấp, vốn, file)
├── RevenueReport[] — PPP only (năm, kỳ, doanh thu, lũy kế, chia sẻ tăng/giảm)
├── InvestorSelection — PPP Tab2 (hình thức, NĐT[] multiselect, QĐ, file)
├── PppContractInfo — PPP Tab2 (TMĐT, Vốn NN breakdown, CSH, Vay, tiến độ, HĐ ký kết)

# NEW catalogs
├── GovernmentAgency — T27 (14 cols: tên, mã, cấp trên, kiểu, LDA, liên hệ, thứ tự)
└── Investor — T28 (6 cols: kiểu NĐT [DN/cá nhân/tổ chức], mã DN/CCCD, tên VN/EN, trạng thái)
```

**Total: ~8-10 new entities, ~4-5 extended entities, 2 new catalogs**

---

## 7. New Admin Catalogs (Detail)

### 7.1 Cơ quan quản lý nhà nước (GovernmentAgency) — T27

| # | Field | Type | Notes |
|---|-------|------|-------|
| 1 | Tên đơn vị | Text | Required |
| 2 | Mã đơn vị | Text | Required, unique |
| 3 | Đơn vị cấp trên | Select | Self-referencing hierarchy |
| 4 | Kiểu đơn vị | Select | "Các Tỉnh; Các Bộ, Ban ngành; Các Quận/huyện; Các Tổng công ty" + more |
| 5 | Nguồn gốc | Text | |
| 6 | LDAServer | Text | |
| 7 | Địa chỉ | Text | |
| 8 | Điện thoại | Text | |
| 9 | Fax | Text | |
| 10 | Email | Text | |
| 11 | Ghi chú | Text | |
| 12 | Thứ tự | Text | Sort order |
| 13 | Thứ tự hiển thị trong BCGSTT | Number | Display order in reports |

**Note:** This is a **hierarchical** catalog (đơn vị cấp trên → self-reference). Heavier than typical flat catalogs. Similar to Organization module's OrgUnit tree.

### 7.2 Nhà đầu tư (Investor) — T28

| # | Field | Type | Values |
|---|-------|------|--------|
| 1 | Kiểu nhà đầu tư | Select | "Doanh nghiệp; Cá nhân; Tổ chức khác" |
| 2 | Mã số doanh nghiệp/CCCD | Text | Business ID or citizen ID |
| 3 | Tên nhà đầu tư tiếng Việt | Text | |
| 4 | Tên nhà đầu tư tiếng Anh | Text | |
| 5 | Trạng thái | Select | "Đang hoạt động; Dừng hoạt động" |

**Used by:** PPP Tab2 NĐT multiselect, DNNN list search filter, NĐT/FDI

### 7.3 Existing catalogs — Minor updates

| Catalog | P1 Status | v1.2 Change |
|---------|-----------|-------------|
| KHLCNT (T26) | Exists | ADD: Tên EN, Ngày ký, Người ký (3 new fields) |
| Tỉnh thành (T29) | Exists in MasterData | ADD: Ngày hiệu lực, Trạng thái (Đang HĐ/Đã sát nhập) |
| Phường/xã (T30) | Exists in MasterData | ADD: Ngày hiệu lực, Trạng thái (same as T29) |

---

## 8. PPP Tab-by-Tab Deep Dive

### Tab 1: Thông tin QĐĐT (T17, 71 fields)

**Sections:**
1. Thông tin dự án: Tên (same as TN)
2. QĐ CTĐT: Có điều chỉnh + Dừng checkboxes (same as ODA), Số/Ngày/Cơ quan QĐ, Sơ bộ TMĐT, File. Insert to list → Sửa/Xóa
3. QĐ Đầu tư/điều chỉnh: Số/Ngày/Cơ quan, **TMĐT = Vốn NN + CSH + Vay** (PPP-unique breakdown). Insert to list → Sửa/Xóa
4. Project fields: Mã, Lĩnh vực (13-item enum), Tình trạng, Nhóm, **Loại hợp đồng** (cascading radio+combo), Dự án thành phần, Cơ quan thẩm quyền, Đơn vị chuẩn bị (NĐT/cơ quan), Mục tiêu
5. Quy mô: Diện tích (ha), Công suất, Hạng mục chính
6. Địa điểm: Multi-row Tỉnh/Xã (same as TN)
7. **Thiết kế thi công** popup (NEW): QĐ info + 7 cost items (thiết bị/xây lắp/GPMB/QLDA/tư vấn/dự phòng/khác) + auto-sum + hạng mục detail table

**Implementation note:** Sections 1-2 similar to ODA pattern (QĐ CTĐT with điều chỉnh/dừng). Section 3 needs PPP-specific capital columns. Section 4 has unique Loại hợp đồng cascading logic. Section 7 (TKTT) is a new reusable popup shared with DNNN.

### Tab 2: Thông tin theo hợp đồng (T18, 30 fields) — PPP UNIQUE

**Sections:**
1. Lựa chọn NĐT: Hình thức (đấu thầu rộng rãi/đàm phán/chỉ định/khác), NĐT multiselect from Investor catalog, QĐ lựa chọn, File
2. Thông tin HĐ: TMĐT (= Vốn NN + CSH + Vay), Vốn NN breakdown (NSTW/NSĐP/NSNN khác → auto-sum Tổng vốn NN), Vốn CSH + Tỷ lệ (auto-calc), Vốn vay
3. Tiến độ: Tiến độ thực hiện, **Thời hạn HĐ** (e.g. "10 năm"), **Cơ chế chia sẻ tăng/giảm doanh thu** (free text)
4. HĐ ký kết: Cơ quan, Số HĐ, Ngày, Thời gian khởi công/hoàn thành

**Entirely new — no equivalent in any P1 module.**

### Tab 3: Tình hình thực hiện (T19, 68 fields)

Pattern similar to TN Tab2 but with:
- "Kế hoạch bố trí vốn NN tham gia" (vs TN "Kế hoạch giao vốn") — key difference: label + "Số vốn NN theo QĐ" (vs TN "Tổng số vốn")
- Same Gói thầu/Hợp đồng popups
- Execution tracking: adds "tiểu dự án sử dụng vốn NN" sub-fields + cumulative from khởi công

### Tab 4: Giải ngân (T20, 14 fields)

Similar to TN Tab3 but 3 capital sources: Vốn NN + Vốn CSH + Vốn vay (each with kỳ + lũy kế), vs TN's 2 sources (ĐTC + Khác).

### Tab 5: Thanh tra (T21, 25 fields)

Same 3 sub-sections as TN (Kiểm tra + Đánh giá + Thanh tra/Kiểm toán) but restructured layout. PPP uses separate Danh sách tables per section instead of TN's inline "Lịch sử bản ghi".

### Tab 6: Khai thác, vận hành (T22, 17 fields) — PPP UNIQUE

**Revenue reporting (PPP-only):**
- Năm báo cáo: combo (year-5 → year)
- Kỳ báo cáo: "6 tháng" / "1 năm"
- Doanh thu kỳ (triệu VNĐ)
- Lũy kế doanh thu (auto-sum)
- Chia sẻ tăng doanh thu / giảm doanh thu
- Khó khăn vướng mắc, Kiến nghị

**Settlement checkboxes (different from TN):**
- Quyết toán vốn ĐTC
- Xác nhận hoàn thành
- QT vốn xây dựng
- Đã đưa vào khai thác vận hành

---

## 9. DNNN Tab1 Deep Dive (T24, 84 fields)

**Sections:**
1. Thông tin dự án: Tên, **Nhà đầu tư** (textbox), **Tỷ lệ vốn NN nắm giữ** (%)
2. QĐ CTĐT: Số, Ngày cấp, File, **TMĐT = CSH + ODA + TCTD** (unique breakdown)
3. QĐ Đầu tư: Same breakdown, + Người ký, Tỷ lệ CSH (auto-calc)
4. **GCNĐKĐT**: Số GCN, Ngày cấp, File, Vốn đầu tư, Vốn CSH, Tỷ lệ → Insert to list
5. Quy mô: Diện tích, Công suất, Hạng mục, Mục tiêu, Tiến độ, Thời gian
6. Địa điểm: Multi-row Tỉnh/Xã + **KKT/KCN/KCX/FTZ/TTTC** field (with tooltip)
7. Thiết kế thi công popup (same as PPP)

**Key differences from TN/ODA:**
- Capital structure: CSH + ODA + TCTD (not NSTW/NSĐP/ĐTC)
- Has GCNĐKĐT section (certificate registration) — new entity
- Has KKT/KCN industrial zone field in locations
- No "Cơ quan chủ quản" combobox (uses NĐT instead)
- No "Thuộc CTMTQG" field (TN-only)

**SRS for remaining DNNN tabs:** "tương tự PPP" (Thanh tra, Khai thác, Tài liệu). THTH tab references exist in function list (R79-R80) but no dedicated field table → assume similar to PPP Tab3 with DNNN capital structure.

---

## 10. NĐT trong nước & FDI — Scope Clarification

### NĐT trong nước (18 functions)
**Missing vs DNNN:** Hợp đồng lựa chọn NĐT (R78), Thiết kế thi công/dự toán (R77), Nghĩa vụ tài chính (R80)
**Present:** GCNĐKĐT (R97), QĐ chấp thuận/điều chỉnh (R96), THTH, Thanh tra, Khai thác, Tài liệu
**No field table** — SRS says "tương tự"

### FDI (18 functions)  
**Same structure as NĐT** — SRS literally says "tương tự NĐT trong nước"
**No field table** — SRS says "tương tự"

**Implementation approach:** Build DNNN as full implementation, then create NĐT as DNNN minus 3 tabs, then FDI as NĐT clone with FDI-specific label changes.

---

## 11. Effort Estimation (Refined)

| Phase | Scope | Effort | Reuse % |
|-------|-------|--------|---------|
| **2G** | New catalogs (GovernmentAgency hierarchy + Investor) + KHLCNT/Province updates | 2-3d | 40% (admin pattern) |
| **2A** | PPP BE domain: PppProject TPT + InvestorSelection + PppContractInfo + RevenueReport + DesignEstimate + extend InvestmentDecision/CapitalPlan/Disbursement/Location | 5-6d | 30% (entities exist, add columns) |
| **2B** | PPP FE: 7 tabs (Tab1 complex, Tab2 entirely new, Tab3-5 similar to TN, Tab6 revenue new, Tab7 shared) + list page | 6-8d | 50% (shared popups/tabs) |
| **2C** | DNNN BE: DnnnProject TPT + RegistrationCertificate + extend Location (KKT) + THTH variant | 3-4d | 50% (PPP patterns available) |
| **2D** | DNNN FE: 6 tabs (Tab1 complex, others mostly shared/PPP-like) + list page | 4-5d | 55% |
| **2E** | NĐT BE+FE: DNNN clone minus 3 tabs | 2-3d | 80% (DNNN clone) |
| **2F** | FDI BE+FE: NĐT clone with label changes | 1-2d | 90% (NĐT clone) |
| **2H** | Testing (unit + E2E) | 3-5d | — |
| **2I** | Buffer & polish | 2-3d | — |
| **Total** | | **28-39d** | ~55% avg |

---

## 12. Recommended Implementation Order

```
Phase 2G (Catalogs) → 2A (PPP BE) → 2B (PPP FE) → 
2C (DNNN BE) → 2D (DNNN FE) →
2E (NĐT BE+FE) → 2F (FDI BE+FE) →
2H (Tests) → 2I (Polish)
```

**Rationale:**
1. Catalogs first — GovernmentAgency & Investor needed by PPP/DNNN
2. PPP first — most complex, establishes DesignEstimate popup + revenue pattern
3. DNNN reuses PPP's DesignEstimate + TKTT popup
4. NĐT/FDI are thin clones (80-90% reuse)

---

## 13. Risk Assessment

| # | Risk | Severity | Mitigation |
|---|------|----------|------------|
| 1 | PPP Tab2 (HĐ dự án) is entirely new pattern — no P1 reference | High | Build as standalone component, review with BA early |
| 2 | Revenue reporting (PPP Tab6) is new periodic data pattern | Medium | Separate RevenueReport entity, simple CRUD per period |
| 3 | GovernmentAgency is hierarchical catalog (self-reference) | Medium | Can reuse Organization module's OrgUnit tree pattern |
| 4 | NĐT/FDI specs are thin ("tương tự") — BA may clarify fields later | Medium | Build as DNNN clone, adjust on BA feedback |
| 5 | 4 new TPT children (total 6) may impact EF query performance | Medium | Benchmark after adding PPP+DNNN |
| 6 | Loại hợp đồng cascading logic (BOT→sub-combo) needs careful FE handling | Low | Ant Design cascade pattern |
| 7 | DesignEstimate popup shared by PPP+DNNN needs generic implementation | Low | Extract as shared component from PPP phase |
| 8 | KHLCNT, Province, Ward need field updates (3-4 new columns) | Low | Migration + update admin UI |

---

## 14. Design Decisions (Approved 2026-04-08, pending BA confirmation)

| # | Question | Decision | Confidence | BA Backlog |
|---|----------|----------|------------|------------|
| Q1 | NĐT/FDI = DNNN clone? | Yes, DNNN minus 3 tabs (TKTT, HĐ NĐT, Nghĩa vụ TC). FDI = NĐT clone. | 95% | Confirm NĐT/FDI field differences |
| Q2 | PPP contract types exhaustive? | Yes, 9 values as dynamic catalog (BOT, BT đất/tiền/không TT, BTO, BOO, O&M, BTL, BLT, HĐ hỗn hợp) | 99% | — |
| Q3 | Revenue PPP-only? | Yes. DNNN/NĐT/FDI dùng TN pattern (checkboxes + tiến độ) | 90% | Confirm DNNN không cần revenue reporting |
| Q4 | Investor system-wide? | Yes, admin-managed, MasterData module | 99% | — |
| Q5 | KHLCNT additive? | Yes, ADD 3 columns (NameEn, SignedDate, SignedBy) | 99% | — |
| Q6 | Province/Ward status? | ADD EffectiveDate + Status columns, keep historical data | 95% | — |
| Q7 | DNNN Nghĩa vụ TC? | **DEFERRED** — no field spec in SRS. Placeholder in THTH tab. | 85% | **MUST confirm**: provide field spec for R80 |
| Q8 | DNNN HĐ NĐT = PPP? | Reuse PPP InvestorSelection section | 80% | Confirm DNNN NĐT scope vs PPP NĐT |
| Q9 | TKTT for TN/ODA? | No, PPP+DNNN only | 99% | — |
| Q10 | CQCQ = GovernmentAgency? | Yes, all types reference T27 catalog. Migrate TN/ODA refs. | 95% | — |

**Status:** Decisions approved by dev lead (thiennd212) 2026-04-08. BA backlog items marked for follow-up confirmation. Implementation proceeds with these assumptions — adjustments via migration if BA overrides.
