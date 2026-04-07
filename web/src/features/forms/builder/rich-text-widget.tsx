// rich-text-widget.tsx — lazy-loaded Quill WYSIWYG editor for RichText fields
import { lazy, Suspense } from 'react';
import { Spin } from 'antd';
import 'react-quill-new/dist/quill.snow.css';

const ReactQuill = lazy(() => import('react-quill-new'));

const MODULES = {
  toolbar: [
    ['bold', 'italic', 'underline'],
    [{ list: 'ordered' }, { list: 'bullet' }],
    ['link'],
    ['clean'],
  ],
};

const MAX_CONTENT_LENGTH = 100_000; // ~100KB HTML cap

interface RichTextWidgetProps {
  value?: string;
  onChange?: (html: string) => void;
  disabled?: boolean;
}

export function RichTextWidget({ value, onChange, disabled }: RichTextWidgetProps) {
  return (
    <Suspense fallback={<Spin size="small" />}>
      <ReactQuill
        theme="snow"
        value={value ?? ''}
        onChange={(content) => {
          if (content.length > MAX_CONTENT_LENGTH) return;
          onChange?.(content);
        }}
        readOnly={disabled}
        modules={MODULES}
      />
    </Suspense>
  );
}
