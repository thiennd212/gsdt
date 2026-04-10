import { Form, Input, Select, Radio, Row, Col, Divider, Button, Space } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { DatePickerMaxToday, ConditionalFieldGroup } from '@/features/shared/components';

// Sub-project type labels updated to match SRS mockup
const SUB_PROJECT_OPTIONS = [
  { value: 0, label: 'Có dự án thành phần' },
  { value: 1, label: 'Không có dự án thành phần' },
  { value: 2, label: 'Là dự án thành phần' },
  { value: 3, label: 'Dự án liên tỉnh' },
];

// Status names that trigger stop/suspension conditional fields
const STOP_STATUS_NAMES = ['Dừng CTĐT', 'Điều chỉnh CTĐT'];

interface CatalogItem { id: string; name: string; isActive?: boolean; }
interface SeedItem { id: string; name: string; }

interface Tab1Zone3Props {
  industrySectors: SeedItem[];
  projectGroups: SeedItem[];
  statuses: SeedItem[];
  managingAuthorities: CatalogItem[];
  nationalTargetPrograms: CatalogItem[];
  projectOwners: CatalogItem[];
  pmus: CatalogItem[];
}

// Zone 3: Phân cấp & Chủ thể — classification, ownership, contact and conditional stop fields.
// Layout per SRS mockup:
//   Row 1: Cơ quan chủ quản | Thuộc CTMTQG | Ngành, lĩnh vực | Nhóm dự án  (4×span6)
//   Row 2: Dự án thành phần radio (4 options)
//   Row 3: Mã kho bạc | Tình trạng dự án | Chủ đầu tư  (3×span8)
//   Row 4: Ban QLDA (+) | GĐ Ban QLDA | SĐT | Email  (4×span6)
//   Row 5: Thời gian thực hiện
export function Tab1Zone3Classification({
  industrySectors,
  projectGroups,
  statuses,
  managingAuthorities,
  nationalTargetPrograms,
  projectOwners,
  pmus,
}: Tab1Zone3Props) {
  // Watch statusId to show/hide conditional stop fields
  const statusId = Form.useWatch('statusId');
  const selectedStatus = statuses.find((s) => s.id === statusId);
  const showStopFields = !!(selectedStatus && STOP_STATUS_NAMES.includes(selectedStatus.name));

  return (
    <>
      {/* Row 1: 4 columns span=6 */}
      <Row gutter={16}>
        <Col span={6}>
          <Form.Item name="managingAuthorityId" label={<span>Cơ quan chủ quản <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng chọn cơ quan chủ quản' }]}>
            <Select
              placeholder="Chọn CQCQ"
              showSearch
              filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
              options={managingAuthorities.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
        <Col span={6}>
          <Form.Item name="nationalTargetProgramId" label={<span>Thuộc CTMTQG <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng chọn CTMTQG' }]}>
            <Select
              placeholder="Chọn CTMTQG"
              allowClear
              showSearch
              filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
              options={nationalTargetPrograms.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
        <Col span={6}>
          <Form.Item name="industrySectorId" label={<span>Ngành, lĩnh vực <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng chọn ngành, lĩnh vực' }]}>
            <Select
              placeholder="Chọn ngành"
              options={industrySectors.map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
        <Col span={6}>
          <Form.Item name="projectGroupId" label={<span>Nhóm dự án <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng chọn nhóm dự án' }]}>
            <Select
              placeholder="Chọn nhóm"
              options={projectGroups.map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
      </Row>

      {/* Row 2: Sub-project type radio group — 4 options per SRS */}
      <Row gutter={16}>
        <Col span={24}>
          <Form.Item name="subProjectType" label={<span>Dự án thành phần <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng chọn loại dự án thành phần' }]}>
            <Radio.Group options={SUB_PROJECT_OPTIONS} />
          </Form.Item>
        </Col>
      </Row>

      {/* Row 3: 3 columns span=8 */}
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="treasuryCode" label="Mã kho bạc">
            <Input placeholder="7XXXXX" maxLength={7} />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="statusId" label={<span>Tình trạng dự án <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng chọn tình trạng' }]}>
            <Select
              placeholder="Chọn tình trạng"
              options={statuses.map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="projectOwnerId" label={<span>Chủ đầu tư <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng chọn chủ đầu tư' }]}>
            <Select
              placeholder="Chọn CĐT"
              showSearch
              filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
              options={projectOwners.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
      </Row>

      {/* Row 4: Ban QLDA with cosmetic "+" button + GĐ Ban QLDA + SĐT + Email (4×span6) */}
      <Row gutter={16}>
        <Col span={6}>
          <Form.Item name="projectManagementUnitId" label={<span>Ban QLDA <span style={{ color: '#ff4d4f' }}>*</span></span>} rules={[{ required: true, message: 'Vui lòng chọn Ban QLDA' }]}>
            <Space.Compact style={{ width: '100%' }}>
              <Select
                placeholder="Chọn Ban QLDA"
                allowClear
                showSearch
                filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
                options={pmus.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))}
                style={{ flex: 1 }}
              />
              {/* Cosmetic "+" button — no action, for SRS layout only */}
              <Button icon={<PlusOutlined />} />
            </Space.Compact>
          </Form.Item>
        </Col>
        <Col span={6}>
          <Form.Item name="pmuDirectorName" label="GĐ Ban QLDA">
            <Input placeholder="Họ tên GĐ" />
          </Form.Item>
        </Col>
        <Col span={6}>
          <Form.Item name="pmuPhone" label="SĐT">
            <Input placeholder="SĐT" />
          </Form.Item>
        </Col>
        <Col span={6}>
          <Form.Item name="pmuEmail" label="Email">
            <Input placeholder="Email" type="email" />
          </Form.Item>
        </Col>
      </Row>

      {/* Row 5: Thời gian thực hiện */}
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="implementationPeriod" label="Thời gian thực hiện">
            <Input placeholder="VD: 2024-2027" />
          </Form.Item>
        </Col>
      </Row>

      {/* Conditional stop/suspension fields — shown only for stop/adjust statuses */}
      <ConditionalFieldGroup value={showStopFields} showWhen={[true]}>
        <Divider orientation="left" plain>Thông tin dừng/điều chỉnh</Divider>
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item name="stopContent" label="Nội dung">
              <Input.TextArea rows={2} placeholder="Nội dung dừng/điều chỉnh" />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="stopDecisionNumber" label="Số QĐ">
              <Input placeholder="Số quyết định" />
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item name="stopDecisionDate" label="Ngày QĐ">
              <DatePickerMaxToday placeholder="Chọn ngày" />
            </Form.Item>
          </Col>
        </Row>
      </ConditionalFieldGroup>
    </>
  );
}
