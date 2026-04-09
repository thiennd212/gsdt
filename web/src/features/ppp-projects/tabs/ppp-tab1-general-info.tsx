import { useEffect, useState } from 'react';
import { Form, Input, Row, Col, Card, Button, Space, message } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useNavigate } from '@tanstack/react-router';
import dayjs from 'dayjs';
import { useCreatePppProject, useUpdatePppProject, usePppProject } from '../ppp-project-api';
import { useAddDesignEstimate, useUpdateDesignEstimate } from '../ppp-project-api';
import { PppTab1DecisionsZone } from './ppp-tab1-decisions-zone';
import { PppTab1LocationsZone } from './ppp-tab1-locations-zone';
import { PppTab1ClassificationForm } from './ppp-tab1-classification-form';
import { DesignEstimatePopup } from '@/features/shared/components/design-estimate-popup';
import type { DesignEstimateDto } from '../ppp-project-types';

interface Tab1Props {
  projectId: string | null;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
  onDirty?: () => void;
  onProjectCreated?: (id: string) => void;
}

// PppTab1GeneralInfo — orchestrates 7 sections across sub-components.
// Sections 3+4 are in PppTab1ClassificationForm; 5 in LocationsZone; 2 in DecisionsZone; 6 in popup.
export function PppTab1GeneralInfo({ projectId, mode, onSaved, onDirty, onProjectCreated }: Tab1Props) {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const isReadonly = mode === 'detail';

  const [estimatePopupOpen, setEstimatePopupOpen] = useState(false);
  const [editingEstimate, setEditingEstimate] = useState<DesignEstimateDto | null>(null);

  // Mutation hooks called at top level (Rules of Hooks)
  const addEstimateMutation = useAddDesignEstimate();
  const updateEstimateMutation = useUpdateDesignEstimate();
  const estimateApiHooks = {
    useAdd: () => addEstimateMutation,
    useUpdate: () => updateEstimateMutation,
  };

  const { data: project } = usePppProject(projectId ?? undefined);
  const createMutation = useCreatePppProject();
  const updateMutation = useUpdatePppProject();
  const saving = createMutation.isPending || updateMutation.isPending;

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
          message.success('Tạo dự án PPP thành công');
          onSaved?.();
          onProjectCreated?.(data.id);
          navigate({ to: `/ppp-projects/${data.id}/edit` });
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
        </Card>

        {/* Section 2: Quyết định — only after project created */}
        {projectId && (
          <Card size="small" title="Quyết định CTĐT & ĐT" style={{ marginBottom: 16 }}>
            <PppTab1DecisionsZone projectId={projectId} disabled={isReadonly} />
          </Card>
        )}

        {/* Sections 3+4: Phân loại, Chủ thể, Quy mô — extracted sub-component */}
        <PppTab1ClassificationForm disabled={isReadonly} />
      </Form>

      {/* Section 5: Địa điểm — outside Form (uses direct API calls, not form values) */}
      {projectId && (
        <Card size="small" title="Địa điểm thực hiện" style={{ marginBottom: 16 }}>
          <PppTab1LocationsZone projectId={projectId} disabled={isReadonly} />
        </Card>
      )}

      {/* Section 6: TKTT — design estimates list + popup trigger */}
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
          <Button onClick={() => navigate({ to: '/ppp-projects' })}>
            Quay lại
          </Button>
        </Space>
      )}
    </div>
  );
}
