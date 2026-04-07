// Properties sidebar — Ant Design Drawer that opens when a node or edge is selected
// Node mode: edit state name, display names, isInitial, isFinal, color, sortOrder
// Edge mode: edit actionName, actionLabelVi, actionLabelEn, requiredRoleCode, conditionsJson

import { useEffect } from 'react';
import { Drawer, Form, Input, Switch, InputNumber, ColorPicker, Button, Space } from 'antd';
import { useTranslation } from 'react-i18next';
import type { WorkflowStateDto, WorkflowTransitionDto } from '../workflow-types';

export type SelectedElement =
  | { type: 'node'; data: WorkflowStateDto }
  | { type: 'edge'; data: WorkflowTransitionDto }
  | null;

interface PropertiesSidebarProps {
  selectedElement: SelectedElement;
  onUpdate: (element: SelectedElement) => void;
  onClose: () => void;
}

export function PropertiesSidebar({ selectedElement, onUpdate, onClose }: PropertiesSidebarProps) {
  const { t } = useTranslation();
  const [form] = Form.useForm();

  // Reset form when selected element changes
  useEffect(() => {
    if (!selectedElement) return;
    if (selectedElement.type === 'node') {
      form.setFieldsValue({ ...selectedElement.data });
    } else {
      form.setFieldsValue({ ...selectedElement.data });
    }
  }, [selectedElement, form]);

  const handleSave = async () => {
    const values = await form.validateFields();
    if (!selectedElement) return;
    if (selectedElement.type === 'node') {
      const color =
        typeof values.color === 'string'
          ? values.color
          : (values.color?.toHexString?.() ?? selectedElement.data.color);
      onUpdate({ type: 'node', data: { ...selectedElement.data, ...values, color } });
    } else {
      onUpdate({ type: 'edge', data: { ...selectedElement.data, ...values } });
    }
  };

  const title =
    selectedElement?.type === 'node'
      ? t('workflow.designer.stateProperties', 'Thuộc tính bước')
      : t('workflow.designer.transitionProperties', 'Thuộc tính hành động');

  return (
    <Drawer
      title={title}
      open={!!selectedElement}
      onClose={onClose}
      width={360}
      footer={
        <Space>
          <Button type="primary" onClick={handleSave} size="small">
            {t('common.apply', 'Áp dụng')}
          </Button>
          <Button onClick={onClose} size="small">
            {t('common.cancel', 'Hủy')}
          </Button>
        </Space>
      }
    >
      <Form form={form} layout="vertical" size="small">
        {selectedElement?.type === 'node' && (
          <>
            <Form.Item name="name" label={t('workflow.stateName', 'Tên hệ thống')} rules={[{ required: true }]}>
              <Input placeholder="e.g. Draft, InReview" />
            </Form.Item>
            <Form.Item name="displayNameVi" label={t('workflow.displayNameVi', 'Tên tiếng Việt')} rules={[{ required: true }]}>
              <Input placeholder="Tên hiển thị tiếng Việt" />
            </Form.Item>
            <Form.Item name="displayNameEn" label={t('workflow.displayNameEn', 'Tên tiếng Anh')} rules={[{ required: true }]}>
              <Input placeholder="English display name" />
            </Form.Item>
            <div style={{ display: 'flex', gap: 16 }}>
              <Form.Item name="isInitial" label={t('workflow.isInitial', 'Bước đầu')} valuePropName="checked">
                <Switch />
              </Form.Item>
              <Form.Item name="isFinal" label={t('workflow.isFinal', 'Bước cuối')} valuePropName="checked">
                <Switch />
              </Form.Item>
            </div>
            <div style={{ display: 'flex', gap: 16 }}>
              <Form.Item name="color" label={t('workflow.color', 'Màu sắc')}>
                <ColorPicker />
              </Form.Item>
              <Form.Item name="sortOrder" label={t('workflow.sortOrder', 'Thứ tự')}>
                <InputNumber min={0} style={{ width: 80 }} />
              </Form.Item>
            </div>
          </>
        )}

        {selectedElement?.type === 'edge' && (
          <>
            <Form.Item name="actionName" label={t('workflow.actionName', 'Tên hành động')} rules={[{ required: true }]}>
              <Input placeholder="e.g. Submit, Approve" />
            </Form.Item>
            <Form.Item name="actionLabelVi" label={t('workflow.actionLabelVi', 'Nhãn tiếng Việt')} rules={[{ required: true }]}>
              <Input placeholder="Nhãn hiển thị tiếng Việt" />
            </Form.Item>
            <Form.Item name="actionLabelEn" label={t('workflow.actionLabelEn', 'Nhãn tiếng Anh')} rules={[{ required: true }]}>
              <Input placeholder="English action label" />
            </Form.Item>
            <Form.Item name="requiredRoleCode" label={t('workflow.requiredRoleCode', 'Vai trò yêu cầu')}>
              <Input placeholder="ROLE_CODE (optional)" />
            </Form.Item>
            <Form.Item name="conditionsJson" label={t('workflow.conditionsJson', 'Điều kiện (JSON)')}>
              <Input.TextArea rows={4} placeholder='{"field":"value"}' />
            </Form.Item>
            <Form.Item
              name="defaultAssigneeId"
              label={t('workflow.defaultAssigneeId', 'Người phê duyệt mặc định')}
            >
              <Input placeholder="UUID người phê duyệt (tuỳ chọn)" />
            </Form.Item>
            <Form.Item name="sortOrder" label={t('workflow.sortOrder', 'Thứ tự')}>
              <InputNumber min={0} style={{ width: 80 }} />
            </Form.Item>
          </>
        )}
      </Form>
    </Drawer>
  );
}
