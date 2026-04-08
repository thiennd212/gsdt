import { Form, Row, Col, Input } from 'antd';
import { MoneyInput } from '@/features/shared/components';

interface TmdtBreakdownProps {
  disabled?: boolean;
}

// PppTab2TmdtBreakdown — capital structure breakdown for PPP contract.
// Auto-calculates: VonNN = NSTW + NSĐP + NSNN; TMĐT = VonNN + CSH + Vay; TỷLệCSH = CSH/TMĐT.
// All derived fields are display-only (disabled); only leaf inputs are submitted.
export function PppTab2TmdtBreakdown({ disabled }: TmdtBreakdownProps) {
  const form = Form.useFormInstance();

  const nsTW = Form.useWatch('nsTW', form) ?? 0;
  const nsDiaPhuong = Form.useWatch('nsDiaPhuong', form) ?? 0;
  const nsNhaNuocKhac = Form.useWatch('nsNhaNuocKhac', form) ?? 0;
  const vonCSH = Form.useWatch('vonCSH', form) ?? 0;
  const vonVay = Form.useWatch('vonVay', form) ?? 0;

  const vonNN = nsTW + nsDiaPhuong + nsNhaNuocKhac;
  const totalInvestment = vonNN + vonCSH + vonVay;
  const tyLeCSH = totalInvestment > 0 ? ((vonCSH / totalInvestment) * 100).toFixed(1) + '%' : '—';

  return (
    <>
      <Row gutter={16}>
        <Col span={8}>
          <Form.Item name="nsTW" label="Vốn NSTW">
            <MoneyInput disabled={disabled} />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="nsDiaPhuong" label="Vốn NSĐP">
            <MoneyInput disabled={disabled} />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="nsNhaNuocKhac" label="NSNN khác">
            <MoneyInput disabled={disabled} />
          </Form.Item>
        </Col>
      </Row>
      <Row gutter={16}>
        <Col span={8}>
          {/* Auto-calc: Vốn NN = NSTW + NSĐP + NSNN khác */}
          <Form.Item label="Tổng Vốn NN (tự tính)">
            <MoneyInput value={vonNN} disabled />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="vonCSH" label="Vốn chủ sở hữu (CSH)">
            <MoneyInput disabled={disabled} />
          </Form.Item>
        </Col>
        <Col span={8}>
          <Form.Item name="vonVay" label="Vốn vay">
            <MoneyInput disabled={disabled} />
          </Form.Item>
        </Col>
      </Row>
      <Row gutter={16}>
        <Col span={8}>
          {/* Auto-calc: TMĐT = VonNN + CSH + Vay */}
          <Form.Item label="Tổng TMĐT (tự tính)">
            <MoneyInput value={totalInvestment} disabled />
          </Form.Item>
        </Col>
        <Col span={8}>
          {/* Auto-calc: Tỷ lệ CSH = CSH / TMĐT */}
          <Form.Item label="Tỷ lệ CSH (tự tính)">
            <Input value={tyLeCSH} disabled />
          </Form.Item>
        </Col>
      </Row>
    </>
  );
}
