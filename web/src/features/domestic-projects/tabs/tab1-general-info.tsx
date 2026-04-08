import { useEffect } from 'react';
import { Form, Input, Select, Radio, Row, Col, Divider, Button, Space, Card, message } from 'antd';
import { useNavigate } from '@tanstack/react-router';
import dayjs from 'dayjs';
import { MoneyInput, DatePickerMaxToday, ConditionalFieldGroup } from '@/features/shared/components';
import {
  useSeedCatalog,
  useDynamicCatalog,
  useCreateDomesticProject,
  useUpdateDomesticProject,
  useDomesticProject,
} from '../domestic-project-api';
import type { DomesticProjectDetail } from '../domestic-project-types';
import { Tab1LocationsZone } from './tab1-locations-zone';
import { Tab1DecisionsZone } from './tab1-decisions-zone';

interface Tab1Props {
  projectId: string | null;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
  onDirty?: () => void;
  /** Callback when project is created — passes new project ID to parent */
  onProjectCreated?: (id: string) => void;
}

// Sub-project type enum matching BE
const SUB_PROJECT_OPTIONS = [
  { value: 0, label: 'Không' },
  { value: 1, label: 'Dự án thành phần' },
  { value: 2, label: 'Tiểu dự án' },
  { value: 3, label: 'Dự án nhóm' },
];

// Status IDs that show conditional stop/suspension fields
const STOP_STATUS_NAMES = ['Dừng CTĐT', 'Điều chỉnh CTĐT'];

// Tab 1: Thông tin chung — 5 zones, ~30+ fields.
// Zone 1: Basic info & policy. Zone 2: Budget. Zone 3: Classification & owner.
// Zone 4: Locations (inline table). Zone 5: Investment decisions (inline form + table).
export function Tab1GeneralInfo({ projectId, mode, onSaved, onDirty, onProjectCreated }: Tab1Props) {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const isReadonly = mode === 'detail';

  // Data queries
  const { data: project } = useDomesticProject(projectId ?? undefined);
  const { data: industrySectors = [] } = useSeedCatalog('industry-sectors');
  const { data: projectGroups = [] } = useSeedCatalog('project-groups');
  const { data: statuses = [] } = useSeedCatalog('domestic-project-statuses');
  const { data: managingAuthorities = [] } = useDynamicCatalog('managing-authorities');
  const { data: nationalTargetPrograms = [] } = useDynamicCatalog('national-target-programs');
  const { data: projectOwners = [] } = useDynamicCatalog('project-owners');
  const { data: pmus = [] } = useDynamicCatalog('project-management-units');

  // Mutations
  const createMutation = useCreateDomesticProject();
  const updateMutation = useUpdateDomesticProject();
  const saving = createMutation.isPending || updateMutation.isPending;

  // Pre-fill form on edit/detail
  useEffect(() => {
    if (project && mode !== 'create') {
      form.setFieldsValue({
        ...project,
        policyDecisionDate: project.policyDecisionDate ? dayjs(project.policyDecisionDate) : null,
        stopDecisionDate: project.stopDecisionDate ? dayjs(project.stopDecisionDate) : null,
      });
    }
  }, [project, mode, form]);

  // Auto-calculate budget fields
  const centralBudget = Form.useWatch('prelimCentralBudget', form) ?? 0;
  const localBudget = Form.useWatch('prelimLocalBudget', form) ?? 0;
  const otherPublicCapital = Form.useWatch('prelimOtherPublicCapital', form) ?? 0;
  const otherCapital = Form.useWatch('prelimOtherCapital', form) ?? 0;
  const publicInvestment = centralBudget + localBudget + otherPublicCapital;
  const totalInvestment = publicInvestment + otherCapital;

  // Check if current status needs stop fields
  const statusId = Form.useWatch('statusId', form);
  const selectedStatus = statuses.find((s) => s.id === statusId);
  const showStopFields = selectedStatus && STOP_STATUS_NAMES.includes(selectedStatus.name);

  async function handleSave() {
    const values = await form.validateFields();
    const payload = {
      ...values,
      policyDecisionDate: values.policyDecisionDate?.format('YYYY-MM-DD') ?? null,
      stopDecisionDate: values.stopDecisionDate?.format('YYYY-MM-DD') ?? null,
    };

    if (mode === 'create') {
      createMutation.mutate(payload, {
        onSuccess: (data) => {
          message.success('Tạo dự án thành công');
          onSaved?.();
          onProjectCreated?.(data.id);
          navigate({ to: `/domestic-projects/${data.id}/edit` });
        },
        onError: () => message.error('Tạo dự án thất bại'),
      });
    } else {
      updateMutation.mutate(
        { id: projectId!, rowVersion: project!.rowVersion, ...payload },
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
        {/* Zone 1: Thông tin cơ bản & Pháp lý */}
        <Card size="small" title="Thông tin cơ bản & Pháp lý" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="projectCode" label="Mã dự án" rules={[{ required: true, message: 'Vui lòng nhập mã dự án' }]}>
                <Input placeholder="Nhập mã dự án" />
              </Form.Item>
            </Col>
            <Col span={16}>
              <Form.Item name="projectName" label="Tên dự án" rules={[{ required: true, message: 'Vui lòng nhập tên dự án' }]}>
                <Input placeholder="Nhập tên dự án" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="policyDecisionNumber" label="Số QĐ chủ trương">
                <Input placeholder="Số quyết định" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="policyDecisionDate" label="Ngày QĐ">
                <DatePickerMaxToday placeholder="Chọn ngày" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="policyDecisionAuthority" label="Cơ quan QĐ">
                <Input placeholder="Cơ quan quyết định" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="policyDecisionPerson" label="Người QĐ">
                <Input placeholder="Người quyết định" />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {/* Zone 2: Tổng mức ĐT & Nguồn vốn */}
        <Card size="small" title="Sơ bộ tổng mức đầu tư & Nguồn vốn" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="prelimCentralBudget" label="Vốn NSTW" rules={[{ required: true }]}>
                <MoneyInput />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="prelimLocalBudget" label="Vốn NSĐP" rules={[{ required: true }]}>
                <MoneyInput />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="prelimOtherPublicCapital" label="Vốn ĐTC khác" rules={[{ required: true }]}>
                <MoneyInput />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item label="Vốn ĐTC (tự tính)">
                <MoneyInput value={publicInvestment} disabled />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="prelimOtherCapital" label="Vốn khác" rules={[{ required: true }]}>
                <MoneyInput />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item label="Sơ bộ TMĐT (tự tính)">
                <MoneyInput value={totalInvestment} disabled />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {/* Zone 3: Phân cấp & Chủ thể */}
        <Card size="small" title="Phân cấp & Chủ thể" style={{ marginBottom: 16 }}>
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
        </Card>
      </Form>

      {/* Zone 4: Địa điểm thực hiện (inline table) */}
      {projectId && (
        <Card size="small" title="Địa điểm thực hiện" style={{ marginBottom: 16 }}>
          <Tab1LocationsZone projectId={projectId} disabled={isReadonly} />
        </Card>
      )}

      {/* Zone 5: QĐ Đầu tư (inline form + table) */}
      {projectId && (
        <Card size="small" title="Quyết định đầu tư" style={{ marginBottom: 16 }}>
          <Tab1DecisionsZone projectId={projectId} disabled={isReadonly} />
        </Card>
      )}

      {/* Action buttons */}
      {!isReadonly && (
        <Space style={{ marginTop: 16 }}>
          <Button type="primary" onClick={handleSave} loading={saving}>
            Lưu thông tin
          </Button>
          <Button onClick={() => navigate({ to: '/domestic-projects' })}>
            Quay lại
          </Button>
        </Space>
      )}
    </div>
  );
}
