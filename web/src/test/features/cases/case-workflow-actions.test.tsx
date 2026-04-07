import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { createElement, type ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

const mockUseSubmitCase = vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false }));
const mockUseAssignCase = vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false }));
const mockUseApproveCase = vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false }));
const mockUseRejectCase = vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false }));
const mockUseCloseCase = vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false }));

vi.mock('@/features/cases/case-api', () => ({
  useSubmitCase: () => mockUseSubmitCase(),
  useAssignCase: () => mockUseAssignCase(),
  useApproveCase: () => mockUseApproveCase(),
  useRejectCase: () => mockUseRejectCase(),
  useCloseCase: () => mockUseCloseCase(),
}));

import { CaseWorkflowActions } from '@/features/cases/case-workflow-actions';
import type { CaseDto } from '@/features/cases/case-types';

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return ({ children }: { children: ReactNode }) =>
    createElement(QueryClientProvider, { client: qc }, children);
}

function makeCaseDto(overrides: Partial<CaseDto> = {}): CaseDto {
  return {
    id: 'case-1',
    caseNumber: 'HS-001',
    title: 'Test',
    description: '',
    type: 'Application',
    status: 'Draft',
    priority: 'Medium',
    tenantId: 'tenant-1',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
    comments: [],
    attachments: [],
    ...overrides,
  } as CaseDto;
}

// Tests expect i18n keys rendered directly (t mock returns key as-is)
describe('CaseWorkflowActions — Draft status', () => {
  it('renders submit button for Draft cases', () => {
    render(<CaseWorkflowActions caseData={makeCaseDto({ status: 'Draft' })} />, {
      wrapper: makeWrapper(),
    });
    expect(screen.getByText('page.cases.workflow.submitBtn')).toBeTruthy();
  });

  it('does not render assign or approve for Draft status', () => {
    render(<CaseWorkflowActions caseData={makeCaseDto({ status: 'Draft' })} />, {
      wrapper: makeWrapper(),
    });
    expect(screen.queryByText('page.cases.workflow.assignBtn')).toBeNull();
    expect(screen.queryByText('page.cases.workflow.approveBtn')).toBeNull();
  });
});

describe('CaseWorkflowActions — Submitted status', () => {
  it('renders assign button for Submitted cases', () => {
    render(<CaseWorkflowActions caseData={makeCaseDto({ status: 'Submitted' })} />, {
      wrapper: makeWrapper(),
    });
    expect(screen.getByText('page.cases.workflow.assignBtn')).toBeTruthy();
  });
});

describe('CaseWorkflowActions — UnderReview status', () => {
  it('renders approve and reject buttons for UnderReview cases', () => {
    render(<CaseWorkflowActions caseData={makeCaseDto({ status: 'UnderReview' })} />, {
      wrapper: makeWrapper(),
    });
    expect(screen.getByText('page.cases.workflow.approveBtn')).toBeTruthy();
    expect(screen.getByText('page.cases.workflow.rejectBtn')).toBeTruthy();
  });
});

describe('CaseWorkflowActions — Approved status', () => {
  it('renders close button for Approved cases', () => {
    render(<CaseWorkflowActions caseData={makeCaseDto({ status: 'Approved' })} />, {
      wrapper: makeWrapper(),
    });
    expect(screen.getByText('page.cases.workflow.closeBtn')).toBeTruthy();
  });
});

describe('CaseWorkflowActions — Closed status', () => {
  it('renders no action buttons for Closed cases', () => {
    render(<CaseWorkflowActions caseData={makeCaseDto({ status: 'Closed' })} />, {
      wrapper: makeWrapper(),
    });
    expect(screen.queryByText('page.cases.workflow.submitBtn')).toBeNull();
    expect(screen.queryByText('page.cases.workflow.assignBtn')).toBeNull();
    expect(screen.queryByText('page.cases.workflow.approveBtn')).toBeNull();
    expect(screen.queryByText('page.cases.workflow.closeBtn')).toBeNull();
  });
});
