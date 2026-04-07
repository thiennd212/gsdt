// signature-widget.tsx — canvas-based signature capture with clear + base64 output
import { useRef, useEffect, useCallback, useState } from 'react';
import { Button, Space } from 'antd';
import SignatureCanvas from 'react-signature-canvas';

interface SignatureWidgetProps {
  value?: string;
  onChange?: (base64: string) => void;
  disabled?: boolean;
}

export function SignatureWidget({ value, onChange, disabled }: SignatureWidgetProps) {
  const sigRef = useRef<SignatureCanvas>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const [canvasWidth, setCanvasWidth] = useState(400);

  // Measure container width for responsive canvas resolution
  useEffect(() => {
    if (!containerRef.current) return;
    const measure = () => {
      const w = containerRef.current?.clientWidth ?? 400;
      setCanvasWidth(Math.max(w - 10, 200)); // 10px for padding
    };
    measure();
    const observer = new ResizeObserver(measure);
    observer.observe(containerRef.current);
    return () => observer.disconnect();
  }, []);

  // Restore saved signature when value changes externally
  useEffect(() => {
    if (!sigRef.current) return;
    if (value && sigRef.current.isEmpty()) {
      sigRef.current.fromDataURL(value);
    }
  }, [value]);

  const handleEnd = useCallback(() => {
    if (!sigRef.current || disabled) return;
    const dataUrl = sigRef.current.toDataURL('image/png');
    // Guard: reject signatures > 500KB base64
    if (dataUrl.length > 500_000) {
      sigRef.current.clear();
      onChange?.('');
      return;
    }
    onChange?.(dataUrl);
  }, [onChange, disabled]);

  const handleClear = useCallback(() => {
    if (!sigRef.current) return;
    sigRef.current.clear();
    onChange?.('');
  }, [onChange]);

  return (
    <Space direction="vertical" style={{ width: '100%' }}>
      <div
        ref={containerRef}
        style={{
          border: '1px solid #d9d9d9',
          borderRadius: 4,
          padding: 4,
          touchAction: 'none', // Prevent scroll conflict on touch devices
          background: disabled ? '#f5f5f5' : '#fff',
        }}
      >
        <SignatureCanvas
          ref={sigRef}
          penColor="black"
          canvasProps={{
            width: canvasWidth,
            height: 150,
            style: { width: '100%', height: 150 },
          }}
          onEnd={handleEnd}
        />
      </div>
      {!disabled && (
        <Button size="small" onClick={handleClear}>
          Xóa chữ ký
        </Button>
      )}
    </Space>
  );
}
