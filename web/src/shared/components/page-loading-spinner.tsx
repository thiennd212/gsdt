import { Spin, Typography } from 'antd';
import { LoadingOutlined } from '@ant-design/icons';

const { Text } = Typography;

interface PageLoadingSpinnerProps {
  /** Show extended message after delay (default: "Hệ thống đang thực hiện yêu cầu") */
  tip?: string;
}

// Full-page loading spinner — shown when data takes >3s to load.
// SRS Section 9: "Hệ thống đang thực hiện yêu cầu" spinner for long operations.
export function PageLoadingSpinner({ tip = 'Hệ thống đang thực hiện yêu cầu...' }: PageLoadingSpinnerProps) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: 300, gap: 16 }}>
      <Spin indicator={<LoadingOutlined style={{ fontSize: 36 }} spin />} />
      <Text type="secondary">{tip}</Text>
    </div>
  );
}
