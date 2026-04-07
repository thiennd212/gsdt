import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { CaseStatusTag } from '@/features/cases/case-status-tag';
import { CASE_STATUS_CONFIG } from '@/features/cases/case-types';
import type { CaseStatus } from '@/features/cases/case-types';

const ALL_STATUSES: CaseStatus[] = [
  'Draft',
  'Submitted',
  'UnderReview',
  'ReturnedForRevision',
  'Approved',
  'Rejected',
  'Closed',
];

describe('CaseStatusTag — renders all statuses', () => {
  ALL_STATUSES.forEach((status) => {
    it(`renders label for status "${status}"`, () => {
      const { unmount } = render(<CaseStatusTag status={status} />);
      // i18n returns the key in test env (no translation loaded)
      const expectedLabel = CASE_STATUS_CONFIG[status].label;
      expect(screen.getByText(expectedLabel)).toBeTruthy();
      unmount();
    });
  });
});

describe('CaseStatusTag — color mapping', () => {
  it('Draft uses "default" color (grey)', () => {
    expect(CASE_STATUS_CONFIG.Draft.color).toBe('default');
  });

  it('Submitted uses "blue" color', () => {
    expect(CASE_STATUS_CONFIG.Submitted.color).toBe('blue');
  });

  it('Approved uses "green" color', () => {
    expect(CASE_STATUS_CONFIG.Approved.color).toBe('green');
  });

  it('Rejected uses "red" color', () => {
    expect(CASE_STATUS_CONFIG.Rejected.color).toBe('red');
  });

  it('UnderReview uses "orange" color', () => {
    expect(CASE_STATUS_CONFIG.UnderReview.color).toBe('orange');
  });

  it('ReturnedForRevision uses "cyan" color', () => {
    expect(CASE_STATUS_CONFIG.ReturnedForRevision.color).toBe('cyan');
  });
});

describe('CaseStatusTag — i18n label keys', () => {
  it('Draft renders i18n key', () => {
    render(<CaseStatusTag status="Draft" />);
    expect(screen.getByText('page.cases.status.Draft')).toBeTruthy();
  });

  it('Approved renders i18n key', () => {
    render(<CaseStatusTag status="Approved" />);
    expect(screen.getByText('page.cases.status.Approved')).toBeTruthy();
  });

  it('Rejected renders i18n key', () => {
    render(<CaseStatusTag status="Rejected" />);
    expect(screen.getByText('page.cases.status.Rejected')).toBeTruthy();
  });
});
