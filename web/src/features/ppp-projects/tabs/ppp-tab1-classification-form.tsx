import { Form, Input, Select, Radio, Row, Col, Card } from 'antd';
import { useSeedCatalog, useDynamicCatalog } from '@/features/domestic-projects/domestic-project-api';
import { PppTab1ContractTypeSelect } from './ppp-tab1-contract-type-select';

interface ClassificationFormProps {
  disabled?: boolean;
}

const SUB_PROJECT_OPTIONS = [
  { value: 0, label: 'Không' },
  { value: 1, label: 'Dự án thành phần' },
  { value: 2, label: 'Tiểu dự án' },
];

// PppTab1ClassificationForm — Section 3 (Phân loại & Chủ thể) + Section 4 (Quy mô).
// Extracted to keep ppp-tab1-general-info.tsx under 200 LOC.
// Must be rendered inside a parent <Form> so Form.Item bindings work correctly.
export function PppTab1ClassificationForm({ disabled }: ClassificationFormProps) {
  const { data: industrySectors = [] } = useSeedCatalog('industry-sectors');
  const { data: projectGroups = [] } = useSeedCatalog('project-groups');
  const { data: statuses = [] } = useSeedCatalog('ppp-project-statuses');
  const { data: managingAuthorities = [] } = useDynamicCatalog('managing-authorities');
  const { data: projectOwners = [] } = useDynamicCatalog('project-owners');

  return (
    <>
      {/* Section 3: Phân loại & Chủ thể */}
      <Card size="small" title="Phân loại & Chủ thể" style={{ marginBottom: 16 }}>
        <Row gutter={16}>
          <Col span={8}>
            <Form.Item name="contractType" label="Loại hợp đồng" rules={[{ required: true, message: 'Vui lòng chọn loại HĐ' }]}>
              <PppTab1ContractTypeSelect disabled={disabled} />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="managingAuthorityId" label="Cơ quan có thẩm quyền" rules={[{ required: true }]}>
              <Select
                placeholder="Chọn CQCQ"
                showSearch
                filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
                options={managingAuthorities.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))}
              />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="projectOwnerId" label="Chủ đầu tư / Nhà ĐT" rules={[{ required: true }]}>
              <Select
                placeholder="Chọn CĐT"
                showSearch
                filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
                options={projectOwners.filter((i) => i.isActive).map((i) => ({ value: i.id, label: i.name }))}
              />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={8}>
            <Form.Item name="industrySectorId" label="Ngành/Lĩnh vực" rules={[{ required: true }]}>
              <Select
                placeholder="Chọn ngành"
                options={industrySectors.map((i) => ({ value: i.id, label: i.name }))}
              />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="projectGroupId" label="Nhóm dự án" rules={[{ required: true }]}>
              <Select
                placeholder="Chọn nhóm"
                options={projectGroups.map((i) => ({ value: i.id, label: i.name }))}
              />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="statusId" label="Tình trạng" rules={[{ required: true }]}>
              <Select
                placeholder="Chọn tình trạng"
                options={statuses.map((i) => ({ value: i.id, label: i.name }))}
              />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={8}>
            <Form.Item name="subProjectType" label="Dự án thành phần">
              <Radio.Group options={SUB_PROJECT_OPTIONS} />
            </Form.Item>
          </Col>
          <Col span={16}>
            <Form.Item name="objectives" label="Mục tiêu dự án">
              <Input.TextArea rows={2} placeholder="Mô tả mục tiêu" />
            </Form.Item>
          </Col>
        </Row>
        <Row gutter={16}>
          <Col span={8}>
            <Form.Item name="implementationPeriod" label="Thời gian thực hiện">
              <Input placeholder="VD: 2025-2035" />
            </Form.Item>
          </Col>
        </Row>
      </Card>

      {/* Section 4: Quy mô */}
      <Card size="small" title="Quy mô dự án" style={{ marginBottom: 16 }}>
        <Row gutter={16}>
          <Col span={8}>
            <Form.Item name="area" label="Diện tích (m²)">
              <Input placeholder="Diện tích" />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="capacity" label="Công suất">
              <Input placeholder="Công suất thiết kế" />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item name="scale" label="Hạng mục chính">
              <Input placeholder="Mô tả hạng mục chính" />
            </Form.Item>
          </Col>
        </Row>
      </Card>
    </>
  );
}
