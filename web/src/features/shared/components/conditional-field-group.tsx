import type { ReactNode } from 'react';

interface ConditionalFieldGroupProps {
  /** Current value of the controlling field */
  value: unknown;
  /** Values that trigger showing the children */
  showWhen: unknown[];
  children: ReactNode;
}

// ConditionalFieldGroup — shows children only when the controlling field value
// matches one of the showWhen values. Used for conditional form sections.
export function ConditionalFieldGroup({ value, showWhen, children }: ConditionalFieldGroupProps) {
  if (!showWhen.includes(value)) return null;
  return <>{children}</>;
}
