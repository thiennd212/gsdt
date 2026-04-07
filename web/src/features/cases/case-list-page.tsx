import { useState, useMemo } from 'react';
import { Table, Button, Space, Popconfirm, message } from 'antd';
import { PlusOutlined, CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useNavigate } from '@tanstack/react-router';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';
import { useCases, useApproveCase, useRejectCase } from './case-api';
import { CaseStatusTag, CasePriorityTag } from './case-status-tag';
import { CaseCreateForm } from './case-create-form';
import { useServerPagination } from '@/core/hooks/use-server-pagination';
import { useDebouncedValue } from '@/core/hooks/use-debounced-value';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminTableToolbar } from '@/shared/components/admin-table-toolbar';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import type { CaseDto, CaseListParams, CaseStatus, CasePriority, CaseType } from './case-types';
import { CASE_TYPE_LABELS, CASE_STATUS_CONFIG, CASE_PRIORITY_CONFIG } from './case-types';

// search is managed separately with debounce; FilterState holds the remaining filter fields
type FilterState = Pick<CaseListParams, 'search' | 'status' | 'priority' | 'type'>;

// buildColumns — returns translated column definitions; called inside component to access t()
function buildColumns(t: (key: string) => string): ColumnsType<CaseDto> {
  return [
    { title: t('page.cases.col.caseNumber'), dataIndex: 'caseNumber', key: 'caseNumber', width: 160, ellipsis: true },
    { title: t('page.cases.col.title'), dataIndex: 'title', key: 'title', ellipsis: true },
    {
      title: t('page.cases.col.type'),
      dataIndex: 'type',
      key: 'type',
      width: 130,
      render: (v: CaseType) => t(CASE_TYPE_LABELS[v]),
    },
    {
      title: t('page.cases.col.status'),
      dataIndex: 'status',
      key: 'status',
      width: 130,
      render: (v: CaseStatus) => <CaseStatusTag status={v} />,
    },
    {
      title: t('page.cases.col.priority'),
      dataIndex: 'priority',
      key: 'priority',
      width: 110,
      render: (v: CasePriority) => <CasePriorityTag priority={v} />,
    },
    {
      title: t('page.cases.col.department'),
      dataIndex: 'assignedToDepartment',
      key: 'assignedToDepartment',
      width: 140,
      ellipsis: true,
      render: (v?: string) => v ?? '—',
    },
    {
      title: t('page.cases.col.createdAt'),
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 130,
      render: (v: string) => dayjs(v).format('DD/MM/YYYY'),
    },
  ];
}

// CaseListPage — paginated case table with filters + create button + batch actions
export function CaseListPage() {
  const { t } = useTranslation();
  const [createOpen, setCreateOpen] = useState(false);
  const [searchInput, setSearchInput] = useState<string>('');
  const [filters, setFilters] = useState<Omit<FilterState, 'search'>>({});
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const navigate = useNavigate();

  // Debounce search input by 300ms — prevents API call on every keystroke
  const debouncedSearch = useDebouncedValue(searchInput, 300);
  const { antPagination, toQueryParams } = useServerPagination(20, [debouncedSearch, filters]);

  const queryParams: CaseListParams = {
    ...toQueryParams(),
    ...filters,
    search: debouncedSearch || undefined,
  };
  const { data, isFetching } = useCases(queryParams);
  const approveCase = useApproveCase();
  const rejectCase = useRejectCase();

  const cases = data?.items ?? [];
  const total = data?.totalCount ?? 0;
  const columns = useMemo(() => buildColumns(t), [t]);

  // Batch approve/reject — Promise.allSettled to report partial failures
  async function handleBatchApprove() {
    const results = await Promise.allSettled(
      selectedIds.map((id) => approveCase.mutateAsync({ id, body: { reason: t('page.cases.batchApproveReason') } }))
    );
    const failed = results.filter((r) => r.status === 'rejected').length;
    if (failed > 0) message.warning(t('page.cases.batchPartialFailure', { failed }));
    setSelectedIds([]);
  }

  async function handleBatchReject() {
    const results = await Promise.allSettled(
      selectedIds.map((id) => rejectCase.mutateAsync({ id, body: { reason: t('page.cases.batchRejectReason') } }))
    );
    const failed = results.filter((r) => r.status === 'rejected').length;
    if (failed > 0) message.warning(t('page.cases.batchPartialFailure', { failed }));
    setSelectedIds([]);
  }

  const rowSelection = {
    selectedRowKeys: selectedIds,
    onChange: (keys: React.Key[]) => setSelectedIds(keys as string[]),
  };

  const statusOptions = (Object.keys(CASE_STATUS_CONFIG) as CaseStatus[]).map((s) => ({
    label: t(CASE_STATUS_CONFIG[s].label), value: s,
  }));
  const priorityOptions = (Object.keys(CASE_PRIORITY_CONFIG) as CasePriority[]).map((p) => ({
    label: t(CASE_PRIORITY_CONFIG[p].label), value: p,
  }));
  const typeOptions = (Object.keys(CASE_TYPE_LABELS) as CaseType[]).map((ct) => ({
    label: t(CASE_TYPE_LABELS[ct]), value: ct,
  }));

  return (
    <div>
      <AdminPageHeader
        title={t('page.cases.title')}
        stats={{ total, label: t('common.items') }}
        actions={
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
            {t('page.cases.createBtn')}
          </Button>
        }
      />
      <AdminContentCard noPadding>
        <AdminTableToolbar
          searchPlaceholder={t('page.cases.searchPlaceholder')}
          searchValue={searchInput}
          onSearchChange={setSearchInput}
          filters={[
            { key: 'status', placeholder: t('page.cases.filter.status'), options: statusOptions },
            { key: 'priority', placeholder: t('page.cases.filter.priority'), options: priorityOptions, width: 130 },
            { key: 'type', placeholder: t('page.cases.filter.type'), options: typeOptions },
          ]}
          filterValues={filters}
          onFilterChange={(k, v) => setFilters((f) => ({ ...f, [k]: v }))}
        />

        {/* Batch action bar — visible only when rows are selected */}
        {selectedIds.length > 0 && (
          <Space style={{ padding: '8px 24px', background: 'rgba(0,123,255,0.04)' }}>
            <span style={{ fontWeight: 500 }}>{selectedIds.length} {t('page.cases.selected')}</span>
            <Popconfirm
              title={t('page.cases.batchApproveConfirm', { count: selectedIds.length })}
              onConfirm={handleBatchApprove}
              okText={t('common.confirm')}
              cancelText={t('common.cancel')}
            >
              <Button icon={<CheckCircleOutlined />} type="primary" loading={approveCase.isPending}>
                {t('page.cases.approveSelected')}
              </Button>
            </Popconfirm>
            <Popconfirm
              title={t('page.cases.batchRejectConfirm', { count: selectedIds.length })}
              onConfirm={handleBatchReject}
              okText={t('common.confirm')}
              cancelText={t('common.cancel')}
              okButtonProps={{ danger: true }}
            >
              <Button icon={<CloseCircleOutlined />} danger loading={rejectCase.isPending}>
                {t('page.cases.rejectSelected')}
              </Button>
            </Popconfirm>
          </Space>
        )}

        <Table<CaseDto>
          rowKey="id"
          rowSelection={rowSelection}
          columns={columns}
          dataSource={cases}
          loading={isFetching}
          size="small"
          scroll={{ x: 900 }}
          pagination={{ ...antPagination, total }}
          onRow={(record) => ({
            onClick: () => navigate({ to: '/cases/$caseId', params: { caseId: record.id } }),
            style: { cursor: 'pointer' },
          })}
        />
      </AdminContentCard>

      <CaseCreateForm open={createOpen} onClose={() => setCreateOpen(false)} />
    </div>
  );
}
