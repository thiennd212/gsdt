import { Radio, Select, Space } from 'antd';
import { PppContractType } from '../ppp-project-types';

interface ContractTypeSelectProps {
  value?: PppContractType;
  onChange?: (value: PppContractType) => void;
  disabled?: boolean;
}

// Radio group determines high-level category (BOT / BT / Khác).
// Selecting BT or Khác reveals a sub-Select for the specific contract type.
const BT_SUBTYPES = [
  { value: PppContractType.BT_Land, label: 'BT (trả đất)' },
  { value: PppContractType.BT_Money, label: 'BT (trả tiền)' },
  { value: PppContractType.BT_NoPayment, label: 'BT (không thanh toán)' },
];

const OTHER_SUBTYPES = [
  { value: PppContractType.BTO, label: 'BTO' },
  { value: PppContractType.BOO, label: 'BOO' },
  { value: PppContractType.OM, label: 'O&M' },
  { value: PppContractType.BTL, label: 'BTL' },
  { value: PppContractType.BLT, label: 'BLT' },
  { value: PppContractType.Mixed, label: 'Hỗn hợp' },
];

function getCategory(value?: PppContractType): 'BOT' | 'BT' | 'other' | undefined {
  if (!value) return undefined;
  if (value === PppContractType.BOT) return 'BOT';
  if ([PppContractType.BT_Land, PppContractType.BT_Money, PppContractType.BT_NoPayment].includes(value)) return 'BT';
  return 'other';
}

// PppTab1ContractTypeSelect — two-level contract type picker (Radio + conditional Select).
export function PppTab1ContractTypeSelect({ value, onChange, disabled }: ContractTypeSelectProps) {
  const category = getCategory(value);

  function handleCategoryChange(cat: 'BOT' | 'BT' | 'other') {
    if (cat === 'BOT') {
      onChange?.(PppContractType.BOT);
    } else if (cat === 'BT') {
      onChange?.(PppContractType.BT_Land); // default sub-type
    } else {
      onChange?.(PppContractType.BTO); // default sub-type
    }
  }

  return (
    <Space direction="vertical" size={8}>
      <Radio.Group
        disabled={disabled}
        value={category}
        onChange={(e) => handleCategoryChange(e.target.value)}
        optionType="button"
        buttonStyle="solid"
        options={[
          { value: 'BOT', label: 'BOT' },
          { value: 'BT', label: 'BT' },
          { value: 'other', label: 'Khác' },
        ]}
      />
      {category === 'BT' && (
        <Select
          disabled={disabled}
          value={value}
          onChange={onChange}
          options={BT_SUBTYPES}
          style={{ width: 220 }}
          placeholder="Chọn loại BT"
        />
      )}
      {category === 'other' && (
        <Select
          disabled={disabled}
          value={value}
          onChange={onChange}
          options={OTHER_SUBTYPES}
          style={{ width: 220 }}
          placeholder="Chọn hình thức hợp đồng"
        />
      )}
    </Space>
  );
}
