// table-field-widget.tsx — editable row grid for TableField type with child field columns
import { useState, useCallback, useEffect } from 'react';
import { Table, Input, InputNumber, Switch, DatePicker, Button, Space } from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import type { FormFieldDto } from '../form-types';

const MAX_ROWS = 50;

interface TableFieldWidgetProps {
  field: FormFieldDto;
  allFields: FormFieldDto[];
  value?: Record<string, unknown>[];
  onChange?: (rows: Record<string, unknown>[]) => void;
  disabled?: boolean;
}

export function TableFieldWidget({
  field,
  allFields,
  value,
  onChange,
  disabled,
}: TableFieldWidgetProps) {
  const [rows, setRows] = useState<Record<string, unknown>[]>(value ?? []);

  // Sync with external value changes
  useEffect(() => {
    if (value) setRows(value);
  }, [value]);

  // Child fields discovered by fieldKey prefix convention (e.g., "table1.col1")
  const resolvedChildren = allFields
    .filter((f) => f.isActive && f.fieldKey.startsWith(`${field.fieldKey}.`))
    .sort((a, b) => a.displayOrder - b.displayOrder);

  const updateCell = useCallback(
    (rowIdx: number, key: string, cellValue: unknown) => {
      setRows((prev) => {
        const updated = [...prev];
        updated[rowIdx] = { ...updated[rowIdx], [key]: cellValue };
        onChange?.(updated);
        return updated;
      });
    },
    [onChange]
  );

  const addRow = useCallback(() => {
    if (rows.length >= MAX_ROWS) return;
    const newRow: Record<string, unknown> = {};
    resolvedChildren.forEach((cf) => {
      const key = cf.fieldKey.split('.').pop() ?? cf.fieldKey;
      newRow[key] = null;
    });
    const updated = [...rows, newRow];
    setRows(updated);
    onChange?.(updated);
  }, [rows, onChange, resolvedChildren]);

  const removeRow = useCallback(
    (idx: number) => {
      const updated = rows.filter((_, i) => i !== idx);
      setRows(updated);
      onChange?.(updated);
    },
    [rows, onChange]
  );

  // Build columns from child fields
  const dataColumns: ColumnsType<Record<string, unknown>> = resolvedChildren.map((cf) => {
    const key = cf.fieldKey.split('.').pop() ?? cf.fieldKey;
    return {
      title: cf.labelVi || key,
      dataIndex: key,
      key,
      render: (val: unknown, _record: Record<string, unknown>, rowIdx: number) => {
        if (disabled) return String(val ?? '');
        switch (cf.type) {
          case 'Number':
            return (
              <InputNumber
                size="small"
                value={val as number}
                onChange={(v) => updateCell(rowIdx, key, v)}
              />
            );
          case 'Date':
            return (
              <DatePicker
                size="small"
                value={val ? dayjs(val as string) : null}
                onChange={(d) => updateCell(rowIdx, key, d?.format('YYYY-MM-DD') ?? null)}
              />
            );
          case 'Boolean':
            return (
              <Switch
                size="small"
                checked={Boolean(val)}
                onChange={(v) => updateCell(rowIdx, key, v)}
              />
            );
          default:
            return (
              <Input
                size="small"
                value={val as string}
                onChange={(e) => updateCell(rowIdx, key, e.target.value)}
              />
            );
        }
      },
    };
  });

  // If no child fields found, show a simple key-value table
  const columns: ColumnsType<Record<string, unknown>> = dataColumns.length
    ? [
        ...dataColumns,
        {
          title: '',
          key: '__actions',
          width: 50,
          render: (_: unknown, __: unknown, rowIdx: number) =>
            !disabled && (
              <Button
                type="text"
                danger
                size="small"
                icon={<DeleteOutlined />}
                onClick={() => removeRow(rowIdx)}
              />
            ),
        },
      ]
    : [
        {
          title: 'Key',
          dataIndex: '__key',
          render: (val: unknown, _: unknown, rowIdx: number) =>
            disabled ? (
              String(val ?? '')
            ) : (
              <Input
                size="small"
                value={val as string}
                onChange={(e) => updateCell(rowIdx, '__key', e.target.value)}
              />
            ),
        },
        {
          title: 'Value',
          dataIndex: '__value',
          render: (val: unknown, _: unknown, rowIdx: number) =>
            disabled ? (
              String(val ?? '')
            ) : (
              <Input
                size="small"
                value={val as string}
                onChange={(e) => updateCell(rowIdx, '__value', e.target.value)}
              />
            ),
        },
        {
          title: '',
          key: '__actions',
          width: 50,
          render: (_: unknown, __: unknown, rowIdx: number) =>
            !disabled && (
              <Button
                type="text"
                danger
                size="small"
                icon={<DeleteOutlined />}
                onClick={() => removeRow(rowIdx)}
              />
            ),
        },
      ];

  return (
    <Space direction="vertical" style={{ width: '100%' }}>
      <Table
        size="small"
        dataSource={rows.map((r, i) => ({ ...r, __rowKey: i }))}
        rowKey="__rowKey"
        columns={columns}
        pagination={false}
        scroll={{ x: true }}
      />
      {!disabled && (
        <Button
          type="dashed"
          size="small"
          icon={<PlusOutlined />}
          onClick={addRow}
          disabled={rows.length >= MAX_ROWS}
          block
        >
          Thêm dòng {rows.length >= MAX_ROWS && `(tối đa ${MAX_ROWS})`}
        </Button>
      )}
    </Space>
  );
}
