import { Upload, Button, message } from 'antd';
import { UploadOutlined } from '@ant-design/icons';
import type { UploadProps, UploadFile } from 'antd';

interface FileUploadFieldProps {
  value?: UploadFile[];
  onChange?: (files: UploadFile[]) => void;
  /** Accepted MIME types (default: .pdf) */
  accept?: string;
  /** Whether at least 1 PDF is required (v1.1) */
  requirePdf?: boolean;
  maxCount?: number;
  disabled?: boolean;
}

// FileUploadField — file upload with PDF validation (v1.1).
// Validates file type on select. Does NOT auto-upload; stores file locally for form submission.
export function FileUploadField({
  value = [],
  onChange,
  accept = '.pdf',
  requirePdf,
  maxCount = 5,
  disabled,
}: FileUploadFieldProps) {
  const uploadProps: UploadProps = {
    beforeUpload: (file) => {
      if (requirePdf && !file.name.toLowerCase().endsWith('.pdf')) {
        message.error('Chỉ chấp nhận file PDF');
        return Upload.LIST_IGNORE;
      }
      // Prevent auto-upload — store file locally
      return false;
    },
    fileList: value,
    onChange: ({ fileList }) => onChange?.(fileList),
    maxCount,
    accept,
  };

  return (
    <Upload {...uploadProps} disabled={disabled}>
      <Button icon={<UploadOutlined />} disabled={disabled}>
        Chọn tệp
      </Button>
    </Upload>
  );
}
