import { useEffect } from 'react';
import { Form, Input, InputNumber, Select, Radio, Row, Col, Divider, Button, Space, Card, message } from 'antd';
import { useNavigate } from '@tanstack/react-router';
import dayjs from 'dayjs';
import { MoneyInput, DatePickerMaxToday } from '@/features/shared/components';
import { useCreateOdaProject, useUpdateOdaProject, useOdaProject, useSeedCatalog, useDynamicCatalog } from '../oda-project-api';

interface OdaTab1Props {
  projectId: string | null;
  mode: 'create' | 'edit' | 'detail';
  onSaved?: () => void;
  onDirty?: () => void;
}

// ODA Tab 1: Thông tin chung — donor, ODA capital breakdown, mechanism %, loan agreements
export function OdaTab1GeneralInfo({ projectId, mode, onSaved, onDirty }: OdaTab1Props) {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const isReadonly = mode === 'detail';

  const { data: project } = useOdaProject(projectId ?? undefined);
  const { data: industrySectors = [] } = useSeedCatalog('industry-sectors');
  const { data: odaStatuses = [] } = useSeedCatalog('oda-project-statuses');
  const { data: odaTypes = [] } = useSeedCatalog('oda-project-types');
  const { data: managingAuthorities = [] } = useDynamicCatalog('managing-authorities');
  const { data: projectOwners = [] } = useDynamicCatalog('project-owners');
  const { data: pmus = [] } = useDynamicCatalog('project-management-units');
  const { data: contractors = [] } = useDynamicCatalog('contractors');

  const createMutation = useCreateOdaProject();
  const updateMutation = useUpdateOdaProject();
  const saving = createMutation.isPending || updateMutation.isPending;

  useEffect(() => {
    if (project && mode !== 'create') {
      form.setFieldsValue({
        ...project,
        policyDecisionDate: project.policyDecisionDate ? dayjs(project.policyDecisionDate) : null,
      });
    }
  }, [project, mode, form]);

  // Auto-calculate ODA total investment
  const odaGrant = Form.useWatch('odaGrantCapital', form) ?? 0;
  const odaLoan = Form.useWatch('odaLoanCapital', form) ?? 0;
  const cpCentral = Form.useWatch('counterpartCentralBudget', form) ?? 0;
  const cpLocal = Form.useWatch('counterpartLocalBudget', form) ?? 0;
  const cpOther = Form.useWatch('counterpartOtherCapital', form) ?? 0;
  const totalInvestment = odaGrant + odaLoan + cpCentral + cpLocal + cpOther;

  // Mechanism complement: grantMechanism + relendingMechanism = 100%
  const grantPercent = Form.useWatch('grantMechanismPercent', form) ?? 0;

  async function handleSave() {
    const values = await form.validateFields();
    const payload = {
      ...values,
      policyDecisionDate: values.policyDecisionDate?.format('YYYY-MM-DD') ?? null,
    };

    if (mode === 'create') {
      createMutation.mutate(payload, {
        onSuccess: (data) => {
          message.success('Tạo dự án ODA thành công');
          onSaved?.();
          navigate({ to: `/oda-projects/${data.id}/edit` });
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
        {/* Zone: Thông tin dự án */}
        <Card size="small" title="Thông tin dự án" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="projectCode" label="Mã dự án" rules={[{ required: true, message: 'Nhập mã DA' }]}>
                <Input placeholder="Mã dự án" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="projectName" label="Tên dự án" rules={[{ required: true, message: 'Nhập tên DA' }]}>
                <Input placeholder="Tên dự án" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="shortName" label="Tên viết tắt" rules={[{ required: true, message: 'Nhập tên viết tắt' }]}>
                <Input placeholder="Tên viết tắt" />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="projectCodeQhns" label="Mã QHNS (v1.1 - không bắt buộc)">
                <Input placeholder="Mã QHNS" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="odaProjectTypeId" label="Loại dự án ODA" rules={[{ required: true }]}>
                <Select placeholder="Chọn loại" options={odaTypes.map((i) => ({ value: i.id, label: i.name }))} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="statusId" label="Tình trạng" rules={[{ required: true }]}>
                <Select placeholder="Chọn tình trạng" options={odaStatuses.map((i) => ({ value: i.id, label: i.name }))} />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {/* Zone: Nhà tài trợ & Chủ thể */}
        <Card size="small" title="Nhà tài trợ & Chủ thể" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="donorId" label="Nhà tài trợ" rules={[{ required: true }]}>
                <Select placeholder="Chọn nhà tài trợ" showSearch allowClear
                  filterOption={(i, o) => String(o?.label ?? '').toLowerCase().includes(i.toLowerCase())}
                  options={contractors.filter((c) => c.isActive).map((c) => ({ value: c.id, label: c.name }))} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="coDonorName" label="Nhà đồng tài trợ">
                <Input placeholder="Tên nhà đồng tài trợ" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="managingAuthorityId" label="Cơ quan chủ quản" rules={[{ required: true }]}>
                <Select placeholder="Chọn CQCQ" showSearch
                  filterOption={(i, o) => String(o?.label ?? '').toLowerCase().includes(i.toLowerCase())}
                  options={managingAuthorities.filter((c) => c.isActive).map((c) => ({ value: c.id, label: c.name }))} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="projectOwnerId" label="Chủ đầu tư" rules={[{ required: true }]}>
                <Select placeholder="Chọn CĐT" showSearch
                  filterOption={(i, o) => String(o?.label ?? '').toLowerCase().includes(i.toLowerCase())}
                  options={projectOwners.filter((c) => c.isActive).map((c) => ({ value: c.id, label: c.name }))} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="projectManagementUnitId" label="Ban QLDA">
                <Select placeholder="Chọn Ban QLDA" allowClear showSearch
                  filterOption={(i, o) => String(o?.label ?? '').toLowerCase().includes(i.toLowerCase())}
                  options={pmus.filter((c) => c.isActive).map((c) => ({ value: c.id, label: c.name }))} />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="industrySectorId" label="Ngành/Lĩnh vực" rules={[{ required: true }]}>
                <Select placeholder="Chọn ngành" options={industrySectors.map((i) => ({ value: i.id, label: i.name }))} />
              </Form.Item>
            </Col>
          </Row>
          <Row gutter={16}>
            <Col span={6}><Form.Item name="pmuDirectorName" label="GĐ Ban QLDA"><Input /></Form.Item></Col>
            <Col span={6}><Form.Item name="pmuPhone" label="SĐT"><Input /></Form.Item></Col>
            <Col span={6}><Form.Item name="pmuEmail" label="Email"><Input type="email" /></Form.Item></Col>
            <Col span={6}><Form.Item name="implementationPeriod" label="Thời gian TH"><Input placeholder="VD: 2025-2030" /></Form.Item></Col>
          </Row>
        </Card>

        {/* Zone: Nguồn vốn ODA */}
        <Card size="small" title="Sơ bộ tổng mức đầu tư & Nguồn vốn ODA" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}><Form.Item name="odaGrantCapital" label="Vốn ODA không hoàn lại" rules={[{ required: true }]}><MoneyInput /></Form.Item></Col>
            <Col span={8}><Form.Item name="odaLoanCapital" label="Vốn vay ODA" rules={[{ required: true }]}><MoneyInput /></Form.Item></Col>
            <Col span={8}><Form.Item label="Tổng mức ĐT (tự tính)"><MoneyInput value={totalInvestment} disabled /></Form.Item></Col>
          </Row>
          <Row gutter={16}>
            <Col span={8}><Form.Item name="counterpartCentralBudget" label="VĐU NSTW" rules={[{ required: true }]}><MoneyInput /></Form.Item></Col>
            <Col span={8}><Form.Item name="counterpartLocalBudget" label="VĐU NSĐP" rules={[{ required: true }]}><MoneyInput /></Form.Item></Col>
            <Col span={8}><Form.Item name="counterpartOtherCapital" label="VĐU khác" rules={[{ required: true }]}><MoneyInput /></Form.Item></Col>
          </Row>
          <Divider orientation="left" plain>Cơ chế tài chính (tổng = 100%)</Divider>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="grantMechanismPercent" label="Cấp phát (%)" rules={[{ required: true }, { type: 'number', min: 0, max: 100 }]}>
                <InputNumber min={0} max={100} style={{ width: '100%' }} addonAfter="%" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="relendingMechanismPercent" label="Vay lại (%)" rules={[{ required: true }, { type: 'number', min: 0, max: 100 }]}>
                <InputNumber min={0} max={100} style={{ width: '100%' }} addonAfter="%" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item label="Tổng">
                <InputNumber value={grantPercent + (Form.useWatch('relendingMechanismPercent', form) ?? 0)} disabled style={{ width: '100%' }} addonAfter="%" />
              </Form.Item>
            </Col>
          </Row>
        </Card>

        {/* Zone: QĐ Chủ trương + Thời gian */}
        <Card size="small" title="Quyết định chủ trương & Thời gian" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={6}><Form.Item name="policyDecisionNumber" label="Số QĐ chủ trương"><Input /></Form.Item></Col>
            <Col span={6}><Form.Item name="policyDecisionDate" label="Ngày QĐ"><DatePickerMaxToday /></Form.Item></Col>
            <Col span={6}><Form.Item name="policyDecisionAuthority" label="Cơ quan QĐ"><Input /></Form.Item></Col>
            <Col span={6}><Form.Item name="policyDecisionPerson" label="Người QĐ"><Input /></Form.Item></Col>
          </Row>
          <Row gutter={16}>
            <Col span={6}><Form.Item name="startYear" label="Năm bắt đầu"><InputNumber min={2000} max={2100} style={{ width: '100%' }} /></Form.Item></Col>
            <Col span={6}><Form.Item name="endYear" label="Năm kết thúc"><InputNumber min={2000} max={2100} style={{ width: '100%' }} /></Form.Item></Col>
          </Row>
        </Card>

        {/* Zone: Điều kiện mua sắm */}
        <Card size="small" title="Điều kiện mua sắm" style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item name="procurementConditionBound" label="Ràng buộc">
                <Radio.Group>
                  <Radio value={true}>Có ràng buộc</Radio>
                  <Radio value={false}>Không ràng buộc</Radio>
                </Radio.Group>
              </Form.Item>
            </Col>
            <Col span={16}>
              <Form.Item name="procurementConditionSummary" label="Mô tả điều kiện">
                <Input.TextArea rows={2} placeholder="Mô tả điều kiện mua sắm" />
              </Form.Item>
            </Col>
          </Row>
        </Card>
      </Form>

      {!isReadonly && (
        <Space style={{ marginTop: 16 }}>
          <Button type="primary" onClick={handleSave} loading={saving}>Lưu thông tin</Button>
          <Button onClick={() => navigate({ to: '/oda-projects' })}>Quay lại</Button>
        </Space>
      )}
    </div>
  );
}
