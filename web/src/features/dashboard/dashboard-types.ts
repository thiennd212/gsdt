// Dashboard KPI types — mirrors KpiDashboardDto from backend

export interface AssigneeKpiDto {
  assigneeName: string;
  totalAssigned: number;
  closed: number;
  avgResolutionDays: number;
}

export interface MonthlyTrendDto {
  year: number;
  month: number;
  newCases: number;
  closedCases: number;
}

export interface KpiDashboardDto {
  totalCases: number;
  openCases: number;
  closedCases: number;
  averageResolutionDays: number;
  casesByType: Record<string, number>;
  casesByStatus: Record<string, number>;
  casesByPriority: Record<string, number>;
  topAssignees: AssigneeKpiDto[];
  monthlyTrend: MonthlyTrendDto[];
}
