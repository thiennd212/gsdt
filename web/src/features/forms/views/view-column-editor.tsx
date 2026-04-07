// view-column-editor.tsx — editable table for configuring view columns
import { Table, Select, Input, InputNumber, Checkbox, Button } from 'antd';
import { PlusOutlined, DeleteOutlined, ArrowUpOutlined, ArrowDownOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import type { FormFieldDto } from '../form-types';
import type { ViewColumnDto, ColumnFormatter } from './view-types';

const FORMATTER_OPTIONS: { value: ColumnFormatter; label: string }[] = [
  { value: 'Text', label: 'Text' },
  { value: 'Date', label: 'Date' },
  { value: 'Currency', label: 'Currency' },
  { value: 'Badge', label: 'Badge' },
  { value: 'Link', label: 'Link' },
];

interface ViewColumnEditorProps {
  fields: FormFieldDto[];
  value?: ViewColumnDto[];
  onChange?: (cols: ViewColumnDto[]) => void;
}

export function ViewColumnEditor({ fields, value = [], onChange }: ViewColumnEditorProps) {
  const fieldOptions = fields
    .filter((f) => !['Section', 'Label', 'Divider'].includes(f.type))
    .map((f) => ({ value: f.fieldKey, label: f.labelVi || f.fieldKey }));

  function update(idx: number, patch: Partial<ViewColumnDto>) {
    const updated = [...value];
    updated[idx] = { ...updated[idx], ...patch };
    onChange?.(updated);
  }

  function remove(idx: number) {
    onChange?.(value.filter((_, i) => i !== idx));
  }

  function move(idx: number, dir: -1 | 1) {
    const target = idx + dir;
    if (target < 0 || target >= value.length) return;
    const updated = [...value];
    [updated[idx], updated[target]] = [updated[target], updated[idx]];
    // Recalc displayOrder
    onChange?.(updated.map((c, i) => ({ ...c, displayOrder: i + 1 })));
  }

  function addColumn() {
    onChange?.([
      ...value,
      {
        fieldName: '',
        displayOrder: value.length + 1,
        sortable: true,
        filterable: false,
        formatter: 'Text',
      },
    ]);
  }

  const columns: ColumnsType<ViewColumnDto> = [
    {
      title: 'Field', dataIndex: 'fieldName', width: 140,
      render: (val: string, _: ViewColumnDto, idx: number) => (
        <Select size="small" style={{ width: 130 }} value={val || undefined}
          onChange={(v) => {
            const f = fields.find((fi) => fi.fieldKey === v);
            update(idx, { fieldName: v, label: f?.labelVi || v });
          }}
          options={fieldOptions} />
      ),
    },
    {
      title: 'Label', dataIndex: 'label', width: 120,
      render: (val: string, _: ViewColumnDto, idx: number) => (
        <Input size="small" value={val} onChange={(e) => update(idx, { label: e.target.value })} />
      ),
    },
    {
      title: 'Sort', dataIndex: 'sortable', width: 50,
      render: (val: boolean, _: ViewColumnDto, idx: number) => (
        <Checkbox checked={val} onChange={(e) => update(idx, { sortable: e.target.checked })} />
      ),
    },
    {
      title: 'Filter', dataIndex: 'filterable', width: 50,
      render: (val: boolean, _: ViewColumnDto, idx: number) => (
        <Checkbox checked={val} onChange={(e) => update(idx, { filterable: e.target.checked })} />
      ),
    },
    {
      title: 'Width', dataIndex: 'width', width: 80,
      render: (val: number | undefined, _: ViewColumnDto, idx: number) => (
        <InputNumber size="small" style={{ width: 70 }} value={val} min={50} max={500}
          onChange={(v) => update(idx, { width: v ?? undefined })} />
      ),
    },
    {
      title: 'Format', dataIndex: 'formatter', width: 100,
      render: (val: string, _: ViewColumnDto, idx: number) => (
        <Select size="small" style={{ width: 90 }} value={val || 'Text'}
          onChange={(v) => update(idx, { formatter: v as ColumnFormatter })}
          options={FORMATTER_OPTIONS} />
      ),
    },
    {
      title: '', key: 'actions', width: 90,
      render: (_: unknown, __: ViewColumnDto, idx: number) => (
        <span>
          <Button type="text" size="small" icon={<ArrowUpOutlined />} disabled={idx <= 0}
            onClick={() => move(idx, -1)} />
          <Button type="text" size="small" icon={<ArrowDownOutlined />} disabled={idx >= value.length - 1}
            onClick={() => move(idx, 1)} />
          <Button type="text" size="small" danger icon={<DeleteOutlined />}
            onClick={() => remove(idx)} />
        </span>
      ),
    },
  ];

  return (
    <>
      <Table
        size="small"
        dataSource={value.map((c, i) => ({ ...c, __key: i }))}
        rowKey="__key"
        columns={columns}
        pagination={false}
        scroll={{ x: true }}
      />
      <Button type="dashed" size="small" icon={<PlusOutlined />}
        onClick={addColumn} style={{ marginTop: 8 }} block>
        Add Column
      </Button>
    </>
  );
}
