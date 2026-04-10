import { Form, Row, Col } from 'antd';
import { MoneyInput } from '@/features/shared/components';

interface Tab1Zone2BudgetProps {
  /** Auto-calculated sum: centralBudget + localBudget + otherPublicCapital */
  publicInvestment: number;
  /** Auto-calculated sum: publicInvestment + otherCapital */
  totalInvestment: number;
}

// Zone 2: Tổng mức đầu tư & Cơ cấu nguồn vốn
// Displays budget inputs with inline "+" / "=" operators and a total highlight box.
// publicInvestment and totalInvestment are computed by the parent via Form.useWatch.
export function Tab1Zone2Budget({ publicInvestment, totalInvestment }: Tab1Zone2BudgetProps) {
  return (
    <>
      {/* Sub-section label */}
      <div style={{ marginBottom: 12 }}>
        <span style={{ fontStyle: 'italic', fontWeight: 600, color: '#1e40af', fontSize: 13 }}>
          Thành phần vốn đầu tư công
        </span>
      </div>

      {/* Row 1: NSTW + NSĐP + Vốn ĐTC khác = Vốn ĐTC (auto-calc, orange border) */}
      <Row gutter={8} align="middle" style={{ marginBottom: 8 }}>
        <Col flex="1 1 0">
          <Form.Item name="prelimCentralBudget" label="Ngân sách trung ương" rules={[{ required: true }]} style={{ marginBottom: 0 }}>
            <MoneyInput />
          </Form.Item>
        </Col>
        <Col flex="none" style={{ paddingTop: 30, fontSize: 20, fontWeight: 700, color: '#6b7280', paddingLeft: 4, paddingRight: 4 }}>+</Col>
        <Col flex="1 1 0">
          <Form.Item name="prelimLocalBudget" label="Ngân sách địa phương" rules={[{ required: true }]} style={{ marginBottom: 0 }}>
            <MoneyInput />
          </Form.Item>
        </Col>
        <Col flex="none" style={{ paddingTop: 30, fontSize: 20, fontWeight: 700, color: '#6b7280', paddingLeft: 4, paddingRight: 4 }}>+</Col>
        <Col flex="1 1 0">
          <Form.Item name="prelimOtherPublicCapital" label="Vốn đầu tư công khác" rules={[{ required: true }]} style={{ marginBottom: 0 }}>
            <MoneyInput />
          </Form.Item>
        </Col>
        <Col flex="none" style={{ paddingTop: 30, fontSize: 20, fontWeight: 700, color: '#f59e0b', paddingLeft: 4, paddingRight: 4 }}>=</Col>
        {/* Auto-calculated, highlighted with orange border */}
        <Col flex="1 1 0">
          <Form.Item label={<span>Vốn đầu tư công (=) <span style={{ color: '#ff4d4f' }}>*</span></span>} style={{ marginBottom: 0 }}>
            <MoneyInput value={publicInvestment} disabled style={{ border: '2px solid #fb923c', borderRadius: 6 }} />
          </Form.Item>
        </Col>
      </Row>

      {/* Row 2: Vốn khác + purple badge label */}
      <Row gutter={8} align="middle" style={{ marginBottom: 16 }}>
        <Col flex="1 1 0">
          <Form.Item name="prelimOtherCapital" label="Vốn khác" rules={[{ required: true }]} style={{ marginBottom: 0 }}>
            <MoneyInput />
          </Form.Item>
        </Col>
        <Col flex="none" style={{ paddingTop: 30, paddingLeft: 8 }}>
          <div style={{ background: '#ede9fe', color: '#5B21B6', padding: '2px 8px', borderRadius: 4, whiteSpace: 'nowrap', fontSize: 13, fontWeight: 600 }}>
            + VỐN ĐTC
          </div>
        </Col>
        <Col flex="3 1 0" />
      </Row>

      {/* Total investment box — purple gradient, large number */}
      <div style={{
        background: 'linear-gradient(135deg, #5B21B6 0%, #7C3AED 100%)',
        borderRadius: 10,
        padding: '20px 28px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        boxShadow: '0 4px 12px rgba(91, 33, 182, 0.3)',
      }}>
        <div style={{ color: '#e9d5ff', fontSize: 13, fontWeight: 600, maxWidth: 200, lineHeight: 1.4 }}>
          SƠ BỘ TỔNG MỨC ĐẦU TƯ<br />
          <span style={{ fontSize: 11, color: '#c4b5fd' }}>(VỐN ĐTC + VỐN KHÁC)</span>
        </div>
        <div style={{ textAlign: 'right' }}>
          <div style={{ color: '#fff', fontSize: 36, fontWeight: 800, letterSpacing: '0.02em', lineHeight: 1 }}>
            {totalInvestment.toLocaleString('vi-VN', { minimumFractionDigits: 3, maximumFractionDigits: 3 })}
          </div>
          <div style={{ color: '#c4b5fd', fontSize: 13, marginTop: 4 }}>TRIỆU VND</div>
        </div>
      </div>
    </>
  );
}
