import { DatePicker } from 'antd';
import type { DatePickerProps } from 'antd';
import dayjs from 'dayjs';

// DatePicker that disables future dates (max = today). Format: DD/MM/YYYY.
export function DatePickerMaxToday(props: Omit<DatePickerProps, 'disabledDate' | 'format'>) {
  return (
    <DatePicker
      {...props}
      format="DD/MM/YYYY"
      disabledDate={(current) => current && current > dayjs().endOf('day')}
      style={{ width: '100%', ...props.style }}
    />
  );
}
