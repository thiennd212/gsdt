import { Input, Select, Flex, Radio } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { useSeedCatalog, useDynamicCatalog } from './domestic-project-api';

interface DomesticProjectListFiltersProps {
  search: string;
  onSearchChange: (value: string) => void;
  actions?: React.ReactNode;
}

// Filter bar for domestic project list — search + catalog dropdowns
export function DomesticProjectListFilters({
  search,
  onSearchChange,
  actions,
}: DomesticProjectListFiltersProps) {
  const { data: projectGroups = [] } = useSeedCatalog('project-groups');
  const { data: statuses = [] } = useSeedCatalog('domestic-project-statuses');
  const { data: managingAuthorities = [] } = useDynamicCatalog('managing-authorities');
  const { data: projectOwners = [] } = useDynamicCatalog('project-owners');

  return (
    <Flex wrap gap={8} align="center" style={{ padding: '12px 24px' }}>
      <Input
        prefix={<SearchOutlined style={{ color: 'var(--gov-text-muted)' }} />}
        placeholder="Tìm kiếm theo tên hoặc mã dự án..."
        value={search}
        onChange={(e) => onSearchChange(e.target.value)}
        allowClear
        style={{ width: 280 }}
      />
      <Select
        placeholder="CQ quản lý"
        allowClear
        showSearch
        filterOption={(input, opt) =>
          String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())
        }
        options={managingAuthorities
          .filter((i) => i.isActive)
          .map((i) => ({ value: i.id, label: i.name }))}
        style={{ width: 180 }}
      />
      <Select
        placeholder="Chủ đầu tư"
        allowClear
        showSearch
        filterOption={(input, opt) =>
          String(opt?.label ?? '').toLowerCase().includes(input.toLowerCase())
        }
        options={projectOwners
          .filter((i) => i.isActive)
          .map((i) => ({ value: i.id, label: i.name }))}
        style={{ width: 180 }}
      />
      <Select
        placeholder="Nhóm DA"
        allowClear
        options={projectGroups.map((i) => ({ value: i.id, label: i.name }))}
        style={{ width: 120 }}
      />
      <Select
        placeholder="Tình trạng"
        allowClear
        options={statuses.map((i) => ({ value: i.id, label: i.name }))}
        style={{ width: 160 }}
      />
      <Radio.Group defaultValue="all" size="small" optionType="button" buttonStyle="solid">
        <Radio.Button value="all">Tất cả</Radio.Button>
        <Radio.Button value="yes">Có</Radio.Button>
        <Radio.Button value="no">Không</Radio.Button>
      </Radio.Group>
      {actions && <div style={{ marginLeft: 'auto', display: 'flex', gap: 8 }}>{actions}</div>}
    </Flex>
  );
}
