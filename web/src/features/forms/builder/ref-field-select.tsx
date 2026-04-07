// ref-field-select.tsx — Select with dynamic options from data source API
import { Select } from 'antd';
import { useFieldOptions } from '../form-api';
import type { FormFieldDto } from '../form-types';

interface RefFieldSelectProps {
  field: FormFieldDto;
  templateId: string;
  value?: string;
  onChange?: (value: string) => void;
}

export function RefFieldSelect({ field, templateId, value, onChange }: RefFieldSelectProps) {
  const { data: dynamicOptions, isLoading } = useFieldOptions(templateId, field.id);

  // Fallback to static options if no dynamic options available
  const options = dynamicOptions?.length
    ? dynamicOptions.map((o) => ({ value: o.value, label: o.labelVi || o.label }))
    : (field.options ?? []).map((o) => ({ value: o.value, label: o.labelVi || o.value }));

  return (
    <Select
      value={value}
      onChange={onChange}
      loading={isLoading}
      options={options}
      showSearch
      filterOption={(input, option) =>
        (option?.label as string)?.toLowerCase().includes(input.toLowerCase()) ?? false
      }
      placeholder={field.labelVi || field.fieldKey}
      allowClear
    />
  );
}
