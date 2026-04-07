// form-field-renderer.tsx — interactive field widgets used inside form preview
// Handles File upload integration with the Files module API

import { useState } from 'react';
import type { ReactNode } from 'react';
import {
  Form, Input, InputNumber, Switch, Select, DatePicker,
  Upload, Button, Typography, Divider, Space,
} from 'antd';
import { UploadOutlined, LinkOutlined } from '@ant-design/icons';
import type { UploadFile, UploadProps } from 'antd';
import type { Rule } from 'antd/es/form';
import { useUploadFile, getFileDownloadUrl } from '@/features/files/file-api';
import type { FormFieldDto } from '../form-types';
import { RefFieldSelect } from './ref-field-select';
import { AddressFieldWidget } from './address-field-widget';
import { RichTextWidget } from './rich-text-widget';
import { SignatureWidget } from './signature-widget';
import { TableFieldWidget } from './table-field-widget';

const { Title, Text, Link } = Typography;

// ---------------------------------------------------------------------------
// File field — interactive upload that calls POST /api/v1/files
// ---------------------------------------------------------------------------

interface FileFieldProps {
  field: FormFieldDto;
  /** Called with fileId after successful upload */
  onUploaded: (fileId: string) => void;
}

function FileFieldWidget({ field, onUploaded }: FileFieldProps) {
  const label = field.labelVi || field.fieldKey;
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [uploadedId, setUploadedId] = useState<string | null>(null);
  const [uploadedName, setUploadedName] = useState<string | null>(null);
  const uploadMutation = useUploadFile();

  const uploadProps: UploadProps = {
    fileList,
    beforeUpload: (file) => {
      const formData = new FormData();
      formData.append('file', file);
      uploadMutation.mutate(formData, {
        onSuccess: (record) => {
          setUploadedId(record.Id);
          setUploadedName(record.OriginalFileName);
          onUploaded(record.Id);
          setFileList([{ uid: record.Id, name: record.OriginalFileName, status: 'done' }]);
        },
        onError: () => {
          setFileList([{ uid: '-1', name: file.name, status: 'error' }]);
        },
      });
      // Prevent antd's default XHR upload
      return false;
    },
    maxCount: 1,
    onRemove: () => {
      setFileList([]);
      setUploadedId(null);
      setUploadedName(null);
    },
  };

  return (
    <Form.Item key={field.id} label={label} required={field.required}>
      <Space direction="vertical" style={{ width: '100%' }}>
        <Upload {...uploadProps}>
          <Button
            icon={<UploadOutlined />}
            loading={uploadMutation.isPending}
            disabled={Boolean(uploadedId)}
          >
            {uploadedId ? 'Uploaded' : 'Choose file'}
          </Button>
        </Upload>
        {uploadedId && uploadedName && (
          <Link href={getFileDownloadUrl(uploadedId)} target="_blank">
            <LinkOutlined style={{ marginRight: 4 }} />
            {uploadedName}
          </Link>
        )}
      </Space>
    </Form.Item>
  );
}

// ---------------------------------------------------------------------------
// Main renderer — maps each FormFieldType to its preview widget
// ---------------------------------------------------------------------------

export interface RenderFieldProps {
  field: FormFieldDto;
  /** Called when a File field completes upload */
  onFileUploaded?: (fieldKey: string, fileId: string) => void;
  /** Template ID for dynamic options (Ref fields) */
  templateId?: string;
  /** All template fields — needed for TableField child columns */
  allFields?: FormFieldDto[];
  /** Extra validation rules merged with default required rule */
  extraRules?: Rule[];
}

/** Returns an interactive Ant Design widget for the given field */
export function renderInteractiveField(
  { field, onFileUploaded, templateId, allFields, extraRules }: RenderFieldProps
): ReactNode {
  const label = field.labelVi || field.fieldKey;
  const required = field.required;

  // Build rules array: required rule + any extra rules from caller
  const rules: Rule[] = [];
  if (required) rules.push({ required: true, message: `${label} là bắt buộc` });
  if (extraRules?.length) rules.push(...extraRules);

  switch (field.type) {
    case 'Text':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label} rules={rules}>
          <Input />
        </Form.Item>
      );
    case 'Number':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label} rules={rules}>
          <InputNumber style={{ width: '100%' }} />
        </Form.Item>
      );
    case 'Date':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label} rules={rules}>
          <DatePicker style={{ width: '100%' }} />
        </Form.Item>
      );
    case 'DateRange':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label} rules={rules}>
          <DatePicker.RangePicker style={{ width: '100%' }} />
        </Form.Item>
      );
    case 'Textarea':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label} rules={rules}>
          <Input.TextArea rows={3} />
        </Form.Item>
      );
    case 'RichText':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label} rules={rules}>
          <RichTextWidget />
        </Form.Item>
      );
    case 'Boolean':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label} valuePropName="checked">
          <Switch />
        </Form.Item>
      );
    case 'File':
      return (
        <FileFieldWidget
          key={field.id}
          field={field}
          onUploaded={(fileId) => onFileUploaded?.(field.fieldKey, fileId)}
        />
      );
    case 'Signature':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label} rules={rules}>
          <SignatureWidget />
        </Form.Item>
      );
    case 'EnumRef':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label} rules={rules}>
          <Select
            options={(field.options ?? []).map((o) => ({
              value: o.value,
              label: o.labelVi || o.value,
            }))}
          />
        </Form.Item>
      );
    case 'InternalRef':
    case 'ExternalRef':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label} rules={rules}>
          <RefFieldSelect field={field} templateId={templateId ?? ''} />
        </Form.Item>
      );
    case 'Formula':
      return (
        <Form.Item key={field.id} label={label}>
          <Input disabled placeholder="(computed)" />
        </Form.Item>
      );
    case 'TableField':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label}>
          <TableFieldWidget field={field} allFields={allFields ?? []} />
        </Form.Item>
      );
    case 'AddressField':
      return (
        <Form.Item key={field.id} name={field.fieldKey} label={label} rules={rules}>
          <AddressFieldWidget />
        </Form.Item>
      );
    case 'Section':
      return (
        <Divider key={field.id} orientation="left">
          <Title level={5} style={{ margin: 0 }}>{label}</Title>
        </Divider>
      );
    case 'Label':
      return (
        <div key={field.id} style={{ marginBottom: 12 }}>
          <Text>{label}</Text>
        </div>
      );
    case 'Divider':
      return <Divider key={field.id} />;
    default:
      return null;
  }
}
