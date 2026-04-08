import { useEffect, useState } from 'react';
import { Form, Input, InputNumber, Row, Col, Card, Button, Space, Select, message } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import dayjs from 'dayjs';
import { useSeedCatalog, useDynamicCatalog } from '@/features/domestic-projects/domestic-project-api';
import { useGovernmentAgencies } from '@/features/admin-catalogs/catalog-api';
import { useCreateDnnnProject, useUpdateDnnnProject, useDnnnProject } from '../dnnn-project-api';
import { useAddDnnnDesignEstimate, useUpdateDnnnDesignEstimate } from '../dnnn-project-api';
import { DnnnTab1DecisionsZone } from './dnnn-tab1-decisions-zone';
import { DnnnTab1LocationsZone } from './dnnn-tab1-locations-zone';
import { DnnnTab1CertificatesZone } from './dnnn-tab1-certificates-zone';
import { DesignEstimatePopup } from '@/features/shared/components/design-estimate-popup';
import type { DesignEstimateDto } from '../dnnn-project-types';

interface Tab1Props {
  projectId: string | null;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
  onDirty?: () => void;
}

// DnnnTab1GeneralInfo — orchestrates DNNN general info sections.
// Sections: basic info → decisions (CTĐT/ĐT) → GCNĐKĐT certificates → locations → design estimates.
export function DnnnTab1GeneralInfo({ projectId, mode, onSaved, onDirty }: Tab1Props) {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const isReadonly = mode === 'detail';

  const [estimatePopupOpen, setEstimatePopupOpen] = useState(false);
  const [editingEstimate, setEditingEstimate] = useState<DesignEstimateDto | null>(null);

  // Mutation hooks called at top level (Rules of Hooks)
  const addEstimateMutation = useAddDnnnDesignEstimate();
  const updateEstimateMutation = useUpdateDnnnDesignEstimate();
  const estimateApiHooks = {
    useAdd: () => addEstimateMutation,
    useUpdate: () => updateEstimateMutation,
  };

  const { data: project } = useDnnnProject(projectId ?? undefined);
  const createMutation = useCreateDnnnProject();
  const updateMutation = useUpdateDnnnProject();
  const saving = createMutation.isPending || updateMutation.isPending;

  const { data: industrySectors = [] } = useSeedCatalog('industry-sectors');
  const { data: projectGroups = [] } = useSeedCatalog('project-groups');
  const { data: statuses = [] } = useSeedCatalog('dnnn-project-statuses');
  const { data: governmentAgencies = [] } = useGovernmentAgencies(false);

  useEffect(() => {
    if (project && mode !== 'create') {
      form.setFieldsValue({ ...project });
    }
  }, [project, mode, form]);

  async function handleSave() {
    const values = await form.validateFields();
    if (mode === 'create') {
      createMutation.mutate(values, {
        onSuccess: (data) => {
          message.success('Tạo dự án DNNN thành công');
          onSaved?.();
          navigate({ to: `/dnnn-projects/${data.id}/edit` });
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

  const designEstimates = project?.designEstimates ?? [];

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
              <Form.Item name="investorName" label="Tên nhà đầu tư / doanh nghiệp DNNN">
                <Input placeholder="Tên NĐT (tối đa 500 ký tự)" maxLength={500} />
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
          <DnnnTab1DecisionsZone projectId={projectId} disabled={isReadonly} />
        </Card>
      )}

      {/* Section 5: GCNĐKĐT — Registration Certificates */}
      {projectId && (
        <Card size="small" title="Giấy chứng nhận đăng ký đầu tư (GCNĐKĐT)" style={{ marginBottom: 16 }}>
          <DnnnTab1CertificatesZone projectId={projectId} disabled={isReadonly} />
        </Card>
      )}

      {/* Section 6: Địa điểm thực hiện */}
      {projectId && (
        <Card size="small" title="Địa điểm thực hiện" style={{ marginBottom: 16 }}>
          <DnnnTab1LocationsZone projectId={projectId} disabled={isReadonly} />
        </Card>
      )}

      {/* Section 7: Thiết kế kỹ thuật - Tổng dự toán */}
      {projectId && (
        <Card
          size="small"
          title="Thiết kế kỹ thuật - Tổng dự toán (TKTT)"
          style={{ marginBottom: 16 }}
          extra={
            !isReadonly && (
              <Button
                size="small"
                icon={<PlusOutlined />}
                onClick={() => { setEditingEstimate(null); setEstimatePopupOpen(true); }}
              >
                Thêm QĐ phê duyệt TKKT
              </Button>
            )
          }
        >
          {designEstimates.length === 0 ? (
            <div style={{ color: 'var(--gov-text-muted)' }}>Chưa có QĐ phê duyệt TKKT</div>
          ) : (
            designEstimates.map((est) => (
              <div
                key={est.id}
                style={{ padding: '4px 0', borderBottom: '1px solid #f0f0f0', cursor: isReadonly ? 'default' : 'pointer' }}
                onClick={() => { if (!isReadonly) { setEditingEstimate(est); setEstimatePopupOpen(true); } }}
              >
                <strong>{est.decisionNumber}</strong>
                {est.decisionDate && (
                  <span style={{ marginLeft: 8, color: 'var(--gov-text-muted)' }}>
                    {dayjs(est.decisionDate).format('DD/MM/YYYY')}
                  </span>
                )}
                <span style={{ marginLeft: 8 }}>{est.decisionAuthority}</span>
              </div>
            ))
          )}
        </Card>
      )}

      {projectId && (
        <DesignEstimatePopup
          projectId={projectId}
          open={estimatePopupOpen}
          onClose={() => setEstimatePopupOpen(false)}
          editingEstimate={editingEstimate}
          apiHooks={estimateApiHooks}
        />
      )}

      {!isReadonly && (
        <Space style={{ marginTop: 16 }}>
          <Button type="primary" onClick={handleSave} loading={saving}>
            Lưu thông tin
          </Button>
          <Button onClick={() => navigate({ to: '/dnnn-projects' })}>
            Quay lại
          </Button>
        </Space>
      )}
    </div>
  );
}
