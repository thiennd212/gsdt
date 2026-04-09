import { useEffect } from 'react';
import { Form, Input, InputNumber, Row, Col, Card, Button, Space, Select, message } from 'antd';
import { useNavigate } from '@tanstack/react-router';
import { useSeedCatalog } from '@/features/domestic-projects/domestic-project-api';
import { useGovernmentAgencies } from '@/features/admin-catalogs/catalog-api';
import { useCreateFdiProject, useUpdateFdiProject, useFdiProject } from '../fdi-project-api';
import { FdiTab1DecisionsZone } from './fdi-tab1-decisions-zone';
import { FdiTab1LocationsZone } from './fdi-tab1-locations-zone';
import { FdiTab1CertificatesZone } from './fdi-tab1-certificates-zone';

interface Tab1Props {
  projectId: string | null;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
  onDirty?: () => void;
}

// FdiTab1GeneralInfo — orchestrates FDI general info sections.
// Sections: basic info → decisions (CTĐT/ĐT) → GCNĐKĐT certificates → locations.
// NO DesignEstimate (TKTT) section — FDI does not have this feature.
export function FdiTab1GeneralInfo({ projectId, mode, onSaved, onDirty }: Tab1Props) {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const isReadonly = mode === 'detail';

  const { data: project } = useFdiProject(projectId ?? undefined);
  const createMutation = useCreateFdiProject();
  const updateMutation = useUpdateFdiProject();
  const saving = createMutation.isPending || updateMutation.isPending;

  const { data: industrySectors = [] } = useSeedCatalog('industry-sectors');
  const { data: projectGroups = [] } = useSeedCatalog('project-groups');
  const { data: statuses = [] } = useSeedCatalog('fdi-project-statuses');
  const { data: governmentAgencies = [] } = useGovernmentAgencies(false);

  useEffect(() => {
    if (project && mode !== 'create') {
      form.setFieldsValue({ ...project });
    }
  }, [project, mode, form]);

  async function handleSave() {
    let values;
    try {
      values = await form.validateFields();
    } catch {
      return;
    }
    if (mode === 'create') {
      createMutation.mutate(values, {
        onSuccess: (data) => {
          message.success('Tạo dự án FDI thành công');
          onSaved?.();
          navigate({ to: `/fdi-projects/${data.id}/edit` });
        },
        onError: () => message.error('Tạo dự án thất bại'),
      });
    } else {
      updateMutation.mutate(
        { id: projectId!, rowVersion: project!.rowVersion, ...values },
        {
          onSuccess: () => { message.success('Cập nhật thành công'); onSaved?.(); },
          onError: () => message.error('Cập nhật thất bại'),
        },
      );
    }
  }

  return (
    <div>
      <Form form={form} layout="vertical" disabled={isReadonly} onValuesChange={() => onDirty?.()}>
        {/* Section 1: Thông tin cơ bản */}
        <Card size="small" title="Thông tin cơ bản" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={6}>
              <Form.Item name="projectCode" label="Mã dự án" rules={[{ required: true, message: 'Vui lòng nhập mã dự án' }]}>
                <Input placeholder="Nhập mã dự án" />
              </Form.Item>
            </Col>
            <Col span={18}>
              <Form.Item name="projectName" label="Tên dự án" rules={[{ required: true, message: 'Vui lòng nhập tên dự án' }]}>
                <Input placeholder="Nhập tên dự án" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="investorName" label="Tên nhà đầu tư nước ngoài">
                <Input placeholder="Tên FDI (tối đa 500 ký tự)" maxLength={500} />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="stateOwnershipRatio" label="Tỷ lệ vốn nhà nước (%)">
                <InputNumber min={0} max={100} precision={2} style={{ width: '100%' }} placeholder="%" />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {/* Section 2: Phân loại & Chủ thể */}
        <Card size="small" title="Phân loại & Chủ thể" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="competentAuthorityId" label="Cơ quan có thẩm quyền" rules={[{ required: true, message: 'Bắt buộc' }]}>
                <Select
                  placeholder="Chọn CQCQ"
                  showSearch
                  allowClear
                  filterOption={(input, opt) => String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())}
                  options={governmentAgencies
                    .filter((a) => a.isActive)
                    .map((a) => ({ value: a.id, label: a.name }))}
                />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="industrySectorId" label="Ngành/Lĩnh vực" rules={[{ required: true, message: 'Bắt buộc' }]}>
                <Select
                  placeholder="Chọn ngành"
                  options={industrySectors.map((i) => ({ value: i.id, label: i.name }))}
                />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="projectGroupId" label="Nhóm dự án" rules={[{ required: true, message: 'Bắt buộc' }]}>
                <Select
                  placeholder="Chọn nhóm"
                  options={projectGroups.map((i) => ({ value: i.id, label: i.name }))}
                />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="statusId" label="Tình trạng" rules={[{ required: true, message: 'Bắt buộc' }]}>
                <Select
                  placeholder="Chọn tình trạng"
                  options={statuses.map((i) => ({ value: i.id, label: i.name }))}
                />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="implementationTimeline" label="Thời gian thực hiện">
                <Input placeholder="VD: 2025–2030" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={24}>
              <Form.Item name="objectives" label="Mục tiêu dự án">
                <Input.TextArea rows={2} placeholder="Mô tả mục tiêu dự án" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={24}>
              <Form.Item name="progressDescription" label="Mô tả tiến độ thực hiện">
                <Input.TextArea rows={2} placeholder="Mô tả tình hình tiến độ" />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {/* Section 3: Quy mô */}
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
      </Form>

      {/* Section 4: Quyết định — only after project exists */}
      {projectId && (
        <Card size="small" title="Quyết định CTĐT & ĐT" style={{ marginBottom: 16 }}>
          <FdiTab1DecisionsZone projectId={projectId} disabled={isReadonly} />
        </Card>
      )}

      {/* Section 5: GCNĐKĐT — Registration Certificates */}
      {projectId && (
        <Card size="small" title="Giấy chứng nhận đăng ký đầu tư (GCNĐKĐT)" style={{ marginBottom: 16 }}>
          <FdiTab1CertificatesZone projectId={projectId} disabled={isReadonly} />
        </Card>
      )}

      {/* Section 6: Địa điểm thực hiện */}
      {projectId && (
        <Card size="small" title="Địa điểm thực hiện" style={{ marginBottom: 16 }}>
          <FdiTab1LocationsZone projectId={projectId} disabled={isReadonly} />
        </Card>
      )}

      {!isReadonly && (
        <Space style={{ marginTop: 16 }}>
          <Button type="primary" onClick={handleSave} loading={saving}>
            Lưu thông tin
          </Button>
          <Button onClick={() => navigate({ to: '/fdi-projects' })}>
            Quay lại
          </Button>
        </Space>
      )}
    </div>
  );
}
