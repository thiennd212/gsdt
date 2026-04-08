import { Breadcrumb } from 'antd';
import { HomeOutlined } from '@ant-design/icons';
import { Link } from '@tanstack/react-router';

interface BreadcrumbItem {
  label: string;
  path?: string;
}

interface PageBreadcrumbProps {
  items: BreadcrumbItem[];
}

// PageBreadcrumb — SRS Section 9: [Menu cấp 0] > [Menu cấp 1] > [Object]
export function PageBreadcrumb({ items }: PageBreadcrumbProps) {
  return (
    <Breadcrumb
      style={{ marginBottom: 12 }}
      items={[
        { title: <Link to="/"><HomeOutlined /></Link> },
        ...items.map((item) => ({
          title: item.path ? <Link to={item.path}>{item.label}</Link> : item.label,
        })),
      ]}
    />
  );
}
