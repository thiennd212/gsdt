// form-submission-helpers.ts — clean form data before submission
// Strips hidden fields (visibility), UI-only types, and internal keys (__rowKey)

import type { FormFieldDto } from './form-types';
import { isFieldVisible } from './form-visibility-helpers';

const UI_ONLY_TYPES = new Set(['Section', 'Label', 'Divider', 'Formula']);

/**
 * Clean form values before API submission:
 * 1. Skip UI-only field types (Section, Label, Divider, Formula)
 * 2. Skip hidden fields (visibility rules evaluate to false)
 * 3. Strip __rowKey from TableField row arrays
 * 4. Skip inactive fields
 */
export function cleanFormData(
  values: Record<string, unknown>,
  fields: FormFieldDto[],
  formValues: Record<string, unknown>
): Record<string, unknown> {
  const cleaned: Record<string, unknown> = {};

  for (const field of fields) {
    if (!field.isActive) continue;
    if (UI_ONLY_TYPES.has(field.type)) continue;
    if (!isFieldVisible(field.visibilityRulesJson, formValues)) continue;

    const val = values[field.fieldKey];
    if (val === undefined) continue;

    // Strip __rowKey from TableField rows
    if (field.type === 'TableField' && Array.isArray(val)) {
      cleaned[field.fieldKey] = (val as Record<string, unknown>[]).map(
        ({ __rowKey: _, ...rest }) => rest
      );
    } else {
      cleaned[field.fieldKey] = val;
    }
  }
  return cleaned;
}
