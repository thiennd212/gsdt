import { useState, useRef } from 'react';
import { Select } from 'antd';

const { Option } = Select;

interface RoleSyncSelectProps {
  userId: string;
  currentRoles: string[];
  assignableRoles: string[];
  syncRolesMutation: {
    mutate: (data: { id: string; roles: string[] }, opts?: { onSettled?: () => void; onSuccess?: () => void; onError?: () => void }) => void;
    isPending: boolean;
  };
  onSuccess?: () => void;
  onError?: () => void;
}

/** Inline multi-select that syncs roles on blur — avoids firing API on every tag change */
export function RoleSyncSelect({
  userId,
  currentRoles,
  assignableRoles,
  syncRolesMutation,
  onSuccess,
  onError,
}: RoleSyncSelectProps) {
  const [selected, setSelected] = useState<string[]>(currentRoles);
  const pendingRef = useRef(false);

  function handleBlur() {
    // Only sync if selection actually changed
    const changed = selected.length !== currentRoles.length ||
      selected.some((r) => !currentRoles.includes(r));
    if (changed && !pendingRef.current) {
      pendingRef.current = true;
      syncRolesMutation.mutate(
        { id: userId, roles: selected },
        {
          onSettled: () => { pendingRef.current = false; },
          onSuccess,
          onError,
        },
      );
    }
  }

  return (
    <Select
      mode="multiple"
      size="small"
      style={{ minWidth: 140, maxWidth: 220 }}
      value={selected}
      loading={syncRolesMutation.isPending}
      onChange={setSelected}
      onBlur={handleBlur}
    >
      {assignableRoles.map((r) => <Option key={r} value={r}>{r}</Option>)}
    </Select>
  );
}
