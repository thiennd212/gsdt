import { Form, Row, Col } from 'antd';
import { MoneyInput } from './money-input';

interface DesignEstimateCostFieldsProps {
  /** Controlled total — passed from parent watching all 7 cost fields */
  totalEstimate: number;
}

// DesignEstimateCostFields — 7 editable cost inputs + auto-sum preview.
// Extracted from design-estimate-popup.tsx to keep that file under 200 LOC.
// Must be rendered inside a parent <Form> so Form.Item bindings work.
export function DesignEstimateCostFields({ totalEstimate }: DesignEstimateCostFieldsProps) {
  return (
    <>
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="costEquipment" label="Chi phí thiết bị">
            <MoneyInput />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="costConstruction" label="Chi phí XD-LĐ">
            <MoneyInput />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="costLandCompensation" label="GPMB">
            <MoneyInput />
          </Form.Item>
        </Col>
      </Row>
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="costManagement" label="Quản lý DA">
            <MoneyInput />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="costConsultancy" label="Tư vấn ĐT-XD">
            <MoneyInput />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="costContingency" label="Dự phòng phí">
            <MoneyInput />
          </Form.Item>
        </Col>
      </Row>
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="costOther" label="Chi phí khác">
            <MoneyInput />
          </Form.Item>
        </Col>
        <Col span={8}>
          {/* Auto-sum preview — display only, NOT submitted to API */}
          <Form.Item label="Tổng dự toán (tự tính)">
            <MoneyInput value={totalEstimate} disabled />
          </Form.Item>
        </Col>
      </Row>
    </>
  );
}
