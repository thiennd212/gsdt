import { Form, Input, Row, Col, Button, message } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { MoneyInput, DatePickerMaxToday, FileUploadField } from '@/features/shared/components';
import { useAddPppDecision } from '../ppp-project-api';

interface DtDecisionFormProps {
  projectId: string;
}

// PppDtDecisionForm — inline add form for QĐ Đầu tư (ĐT) with PPP capital breakdown.
// Extracted from ppp-tab1-decisions-zone to stay under 200 LOC per file.
export function PppDtDecisionForm({ projectId }: DtDecisionFormProps) {
  const [form] = Form.useForm();
  const addMutation = useAddPppDecision();

  async function handleAdd() {
    const values = await form.validateFields();
    addMutation.mutate(
      {
        projectId,
        decisionType: 1,
        ...values,
        decisionDate: values.decisionDate?.format('YYYY-MM-DD'),
      },
      {
        onSuccess: () => { message.success('Thêm QĐ ĐT thành công'); form.resetFields(); },
        onError: () => message.error('Thêm thất bại'),
      },
    );
  }

  return (
    <Form form={form} layout="vertical" style={{ marginBottom: 12 }}>
      <Row gutter={16}>
        <Col span={6}>
          <Form.Item name="decisionNumber" label="Số QĐ" rules={[{ required: true, message: 'Bắt buộc' }]}>
            <Input placeholder="Số quyết định" size="small" />
          </Form.Item>
        </Col>
        <Col span={5}>
          <Form.Item name="decisionDate" label="Ngày QĐ">
            <DatePickerMaxToday placeholder="Ngày" size="small" style={{ width: '100%' }} />
          </Form.Item>
        </Col>
        <Col span={7}>
          <Form.Item name="decisionAuthority" label="Cơ quan QĐ">
            <Input placeholder="Cơ quan" size="small" />
          </Form.Item>
        </Col>
        <Col span={6}>
          <Form.Item name="signer" label="Người ký">
            <Input placeholder="Người ký" size="small" />
          </Form.Item>
        </Col>
      </Row>
      <Row gutter={16}>
        <Col span={4}>
          <Form.Item name="nsTW" label="Vốn NSTW">
            <MoneyInput size="small" />
          </Form.Item>
        </Col>
        <Col span={4}>
          <Form.Item name="nsDiaPhuong" label="Vốn NSĐP">
            <MoneyInput size="small" />
          </Form.Item>
        </Col>
        <Col span={4}>
          <Form.Item name="nsNhaNuocKhac" label="NSNN khác">
            <MoneyInput size="small" />
          </Form.Item>
        </Col>
        <Col span={4}>
          <Form.Item name="vonCSH" label="Vốn CSH">
            <MoneyInput size="small" />
          </Form.Item>
        </Col>
        <Col span={4}>
          <Form.Item name="vonVay" label="Vốn vay">
            <MoneyInput size="small" />
          </Form.Item>
        </Col>
        <Col span={4}>
          <Form.Item name="fileId" label="Văn bản">
            <FileUploadField accept=".pdf" maxCount={1} />
          </Form.Item>
        </Col>
      </Row>
      <Button size="small" icon={<PlusOutlined />} onClick={handleAdd} loading={addMutation.isPending}>
        Thêm QĐ ĐT
      </Button>
    </Form>
  );
}
