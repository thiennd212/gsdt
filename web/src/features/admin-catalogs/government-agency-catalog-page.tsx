import { useState, useMemo } from 'react';
import { Tree, Button, Space, Popconfirm, message, Tag, Spin } from 'antd';
import type { DataNode } from 'antd/es/tree';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { AdminPageHeader } from '@/shared/components/admin-page-header';
import { AdminContentCard } from '@/shared/components/admin-content-card';
import { AdminTableToolbar } from '@/shared/components/admin-table-toolbar';
import { GOVERNMENT_AGENCY_META } from './catalog-config';
import { apiClient } from '@/core/api';
import {
  useGovernmentAgencyTree,
  useCreateGovernmentAgency,
  useUpdateGovernmentAgency,
  useDeleteGovernmentAgency,
} from './catalog-api';
import { GovernmentAgencyFormModal } from './government-agency-form-modal';
import type {
  GovernmentAgencyDto,
  GovernmentAgencyTreeNode,
  CreateGovernmentAgencyRequest,
  UpdateGovernmentAgencyRequest,
} from './catalog-types';

// Build Ant Design TreeDataNode from GovernmentAgencyTreeNode, filtering by search term
function buildTreeData(
  nodes: GovernmentAgencyTreeNode[],
  search: string,
  onEdit: (item: GovernmentAgencyDto) => void,
  onDelete: (id: string) => void,
): DataNode[] {
  const term = search.toLowerCase().trim();

  return nodes
    .filter((node) => {
      if (!term) return true;
      const matchSelf =
        node.name.toLowerCase().includes(term) ||
        node.code.toLowerCase().includes(term);
      // Recurse: check if any descendant matches the search term
      const hasMatchingChild = buildTreeData(node.children, search, onEdit, onDelete).length > 0;
      return matchSelf || hasMatchingChild;
    })
    .map((node) => ({
      key: node.id,
      title: (
        <Space size={6}>
          <span style={{ fontWeight: 500 }}>{node.name}</span>
          <span style={{ color: 'var(--gov-text-muted)', fontSize: 12 }}>
            ({node.code})
          </span>
          {node.agencyType && (
            <Tag color="blue" style={{ fontSize: 11, padding: '0 4px' }}>
              {node.agencyType}
            </Tag>
          )}
          {!node.isActive && <Tag color="default">Ngừng</Tag>}
          <Space size={2} style={{ marginLeft: 4 }}>
            <Button
              size="small"
              type="text"
              icon={<EditOutlined />}
              onClick={(e) => { e.stopPropagation(); onEdit(node); }}
            />
            <Popconfirm
              title="Xác nhận xóa"
              description="Bạn có chắc chắn muốn xóa cơ quan này?"
              onConfirm={(e) => { e?.stopPropagation(); onDelete(node.id); }}
              onCancel={(e) => e?.stopPropagation()}
              okText="Xóa"
              cancelText="Hủy"
            >
              <Button
                size="small"
                type="text"
                danger
                icon={<DeleteOutlined />}
                onClick={(e) => e.stopPropagation()}
              />
            </Popconfirm>
          </Space>
        </Space>
      ),
      children: buildTreeData(node.children, search, onEdit, onDelete),
    }));
}

// Count total agencies across all tree nodes
function countNodes(nodes: GovernmentAgencyTreeNode[]): number {
  return nodes.reduce((acc, n) => acc + 1 + countNodes(n.children), 0);
}

// Dedicated page for GovernmentAgency catalog — renders data as a hierarchical tree
export function GovernmentAgencyCatalogPage() {
  const [search, setSearch] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<GovernmentAgencyDto | null>(null);

  const { data: tree = [], isLoading } = useGovernmentAgencyTree();
  const createMutation = useCreateGovernmentAgency();
  const updateMutation = useUpdateGovernmentAgency();
  const deleteMutation = useDeleteGovernmentAgency();

  const totalCount = useMemo(() => countNodes(tree), [tree]);

  const treeData = useMemo(
    () => buildTreeData(tree, search, openEdit, handleDelete),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [tree, search],
  );

  function openCreate() {
    setEditingItem(null);
    setModalOpen(true);
  }

  // Fetch full details via API before opening edit (tree node has partial fields only)
  async function openEdit(item: GovernmentAgencyDto) {
    try {
      const res = await apiClient.get<GovernmentAgencyDto>(
        `/masterdata/government-agencies/${item.id}`,
      );
      setEditingItem(res.data);
      setModalOpen(true);
    } catch {
      message.error('Không thể tải thông tin chi tiết');
    }
  }

  function closeModal() {
    setModalOpen(false);
    setEditingItem(null);
  }

  function handleSubmit(
    values:
      | CreateGovernmentAgencyRequest
      | (UpdateGovernmentAgencyRequest & { id: string }),
  ) {
    const isEdit = 'id' in values;
    const onSuccess = () => {
      message.success(isEdit ? 'Cập nhật thành công' : 'Thêm mới thành công');
      closeModal();
    };
    const onError = () => message.error('Thao tác thất bại, vui lòng thử lại');

    if (isEdit) {
      updateMutation.mutate(values, { onSuccess, onError });
    } else {
      createMutation.mutate(values, { onSuccess, onError });
    }
  }

  function handleDelete(id: string) {
    deleteMutation.mutate(id, {
      onSuccess: () => message.success('Xóa thành công'),
      onError: () => message.error('Xóa thất bại'),
    });
  }

  const saving = createMutation.isPending || updateMutation.isPending;

  return (
    <div>
      <AdminPageHeader
        title={GOVERNMENT_AGENCY_META.label}
        description={GOVERNMENT_AGENCY_META.description}
        icon={GOVERNMENT_AGENCY_META.icon}
        stats={{ total: totalCount }}
      />
      <AdminContentCard>
        <AdminTableToolbar
          searchPlaceholder="Tìm kiếm theo tên hoặc mã cơ quan..."
          searchValue={search}
          onSearchChange={setSearch}
          actions={
            <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
              Thêm mới
            </Button>
          }
        />
        {isLoading ? (
          <Spin style={{ display: 'block', margin: '40px auto' }} />
        ) : (
          <Tree
            treeData={treeData}
            defaultExpandAll={Boolean(search)}
            expandedKeys={search ? treeData.map((n) => n.key as string) : undefined}
            showLine={{ showLeafIcon: false }}
            style={{ padding: '8px 0' }}
          />
        )}
      </AdminContentCard>
      <GovernmentAgencyFormModal
        open={modalOpen}
        editingItem={editingItem}
        saving={saving}
        onSubmit={handleSubmit}
        onCancel={closeModal}
      />
    </div>
  );
}
