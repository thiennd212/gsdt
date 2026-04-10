import { Form, Input, Select, Radio, Row, Col, Divider } from 'antd';
import { DatePickerMaxToday, ConditionalFieldGroup } from '@/features/shared/components';

// Sub-project type enum matching BE
const SUB_PROJECT_OPTIONS = [
  { value: 0, label: 'Không' },
  { value: 1, label: 'Dự án thành phần' },
  { value: 2, label: 'Tiểu dự án' },
  { value: 3, label: 'Dự án nhóm' },
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
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="managingAuthorityId" label="Cơ quan chủ quản" rules={[{ required: true }]}>
            <Select
              placeholder="Chọn CQCQ"
              showSearch
              filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
              options={managingAuthorities.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="nationalTargetProgramId" label="CTMTQG">
            <Select
              placeholder="Chọn CTMTQG"
              allowClear
              showSearch
              filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
              options={nationalTargetPrograms.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="industrySectorId" label="Ngành/Lĩnh vực" rules={[{ required: true }]}>
            <Select
              placeholder="Chọn ngành"
              options={industrySectors.map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
      </Row>
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="projectGroupId" label="Nhóm dự án" rules={[{ required: true }]}>
            <Select
              placeholder="Chọn nhóm"
              options={projectGroups.map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="subProjectType" label="Dự án thành phần" rules={[{ required: true }]}>
            <Radio.Group options={SUB_PROJECT_OPTIONS} />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="treasuryCode" label="Mã kho bạc">
            <Input placeholder="Mã kho bạc" />
          </Form.Item>
        </Col>
      </Row>
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="statusId" label="Tình trạng dự án" rules={[{ required: true }]}>
            <Select
              placeholder="Chọn tình trạng"
              options={statuses.map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="projectOwnerId" label="Chủ đầu tư" rules={[{ required: true }]}>
            <Select
              placeholder="Chọn CĐT"
              showSearch
              filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
              options={projectOwners.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="projectManagementUnitId" label="Ban QLDA">
            <Select
              placeholder="Chọn Ban QLDA"
              allowClear
              showSearch
              filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
              options={pmus.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))}
            />
          </Form.Item>
        </Col>
      </Row>
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="pmuDirectorName" label="GĐ Ban QLDA">
            <Input placeholder="Họ tên GĐ" />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="pmuPhone" label="Số điện thoại">
            <Input placeholder="SĐT" />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="pmuEmail" label="Email">
            <Input placeholder="Email" type="email" />
          </Form.Item>
        </Col>
      </Row>
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="implementationPeriod" label="Thời gian thực hiện">
            <Input placeholder="VD: 2025-2030" />
          </Form.Item>
        </Col>
      </Row>

      {/* Conditional stop/suspension fields */}
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
