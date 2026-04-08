import { InputNumber } from 'antd';
import type { InputNumberProps } from 'antd';

interface MoneyInputProps extends Omit<InputNumberProps<number>, 'formatter' | 'parser'> {
  /** Unit suffix shown after value (default: "triệu VNĐ") */
  unit?: string;
}

// Format number with dot thousands separator (Vietnamese convention: 1.234.567)
function formatMoney(value: number | string | undefined): string {
  if (value === undefined || value === '') return '';
  const num = typeof value === 'string' ? parseFloat(value) : value;
  if (isNaN(num)) return '';
  return num.toLocaleString('vi-VN');
}

// Parse formatted string back to number
function parseMoney(value: string | undefined): number {
  if (!value) return 0;
  return parseFloat(value.replace(/\./g, '').replace(',', '.')) || 0;
}

// MoneyInput — decimal input with Vietnamese number format (dot separator), right-aligned.
// Unit suffix: "triệu VNĐ" by default. Used for budget/capital fields.
export function MoneyInput({ unit = 'triệu VNĐ', style, ...props }: MoneyInputProps) {
  return (
    <InputNumber<number>
      {...props}
      formatter={formatMoney}
      parser={parseMoney}
      addonAfter={unit}
      style={{ width: '100%', textAlign: 'right', ...style }}
      min={0}
      precision={2}
    />
  );
}
