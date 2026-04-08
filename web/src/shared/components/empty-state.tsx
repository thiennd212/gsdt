import { Empty, Typography } from 'antd';

const { Text } = Typography;

interface EmptyStateProps {
  /** Context-specific message (default: "Chưa có dữ liệu") */
  message?: string;
  /** Optional description below the message */
  description?: string;
}

// EmptyState — shown when a table/list has no data.
// SRS Section 9: "Chưa có dữ liệu" with context-specific text.
export function EmptyState({ message = 'Chưa có dữ liệu', description }: EmptyStateProps) {
  return (
    <Empty
      image={Empty.PRESENTED_IMAGE_SIMPLE}
      description={
        <div>
          <Text>{message}</Text>
          {description && <br />}
          {description && <Text type="secondary" style={{ fontSize: 12 }}>{description}</Text>}
        </div>
      }
    />
  );
}
