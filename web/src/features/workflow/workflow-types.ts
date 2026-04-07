// Workflow feature types — mirrors backend DTOs for definitions, states, transitions, configs

export interface WorkflowStateDto {
  id: string;
  name: string;
  displayNameVi: string;
  displayNameEn: string;
  isInitial: boolean;
  isFinal: boolean;
  color: string;
  sortOrder: number;
  slaHours?: number | null;
  autoTransitionOnTimeoutId?: string | null;
}

export interface WorkflowTransitionDto {
  id: string;
  fromStateId: string;
  toStateId: string;
  actionName: string;
  actionLabelVi: string;
  actionLabelEn: string;
  requiredRoleCode?: string | null;
  conditionsJson?: string | null;
  sortOrder: number;
  isRecall: boolean;
  /** UUID of the default approver for parallel-branch child steps on this transition */
  defaultAssigneeId?: string | null;
}

export interface WorkflowDefinitionDto {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  tenantId: string;
  createdAt: string;
  definitionKey: string;
  version: number;
  isLatest: boolean;
  states: WorkflowStateDto[];
  transitions: WorkflowTransitionDto[];
}

export interface WorkflowDefinitionListDto {
  id: string;
  name: string;
  description?: string;
  entityType: string;
  isActive: boolean;
  definitionKey: string;
  version: number;
  isLatest: boolean;
  stateCount: number;
  transitionCount: number;
  createdAt: string;
}

export interface CreateWorkflowRequest {
  name: string;
  description?: string;
  entityType: string;
  states: GraphStateInput[];
  transitions: GraphTransitionInput[];
}

export interface GraphStateInput {
  name: string;
  displayNameVi: string;
  displayNameEn: string;
  isInitial: boolean;
  isFinal: boolean;
  color: string;
  sortOrder: number;
  slaHours?: number | null;
  autoTransitionOnTimeoutId?: string | null;
}

export interface GraphTransitionInput {
  fromStateName: string;
  toStateName: string;
  actionName: string;
  actionLabelVi: string;
  actionLabelEn: string;
  requiredRoleCode?: string | null;
  conditionsJson?: string | null;
  sortOrder: number;
  isRecall?: boolean;
  /** UUID of the default approver for parallel-branch child steps on this transition */
  defaultAssigneeId?: string | null;
}

export interface SaveGraphRequest {
  states: GraphStateInput[];
  transitions: GraphTransitionInput[];
}

export interface WorkflowNotificationConfigDto {
  id: string;
  definitionId: string;
  actionName: string;
  channel: string;
  recipientType: string;
  recipientValue: string;
  subject: string;
  bodyTemplate: string;
  isActive: boolean;
}

export interface CreateNotificationConfigRequest {
  actionName: string;
  channel: string;
  recipientType: string;
  recipientValue: string;
  subject: string;
  bodyTemplate: string;
}

export interface WorkflowAssignmentRuleDto {
  id: string;
  tenantId: string;
  workflowDefinitionId: string;
  entityType?: string | null;
  entitySubType?: string | null;
  specificity: number;
  priority: number;
  isActive: boolean;
  description?: string | null;
  effectiveFrom: string;
  effectiveUntil?: string | null;
}

export interface CreateAssignmentRuleRequest {
  workflowDefinitionId: string;
  entityType?: string | null;
  entitySubType?: string | null;
  priority: number;
  description?: string | null;
  effectiveFrom: string;
  effectiveUntil?: string | null;
}

// ─── Task & Instance types ─────────────────────────────────────────────────────

export interface WorkflowTaskDto {
  id: string;
  workflowInstanceId: string;
  title: string;
  description?: string | null;
  assigneeId?: string | null;
  status: number; // 0=Open, 1=InProgress, 2=Done, 3=Cancelled
  priority: number; // 0=Low, 1=Normal, 2=High, 3=Urgent
  dueDate?: string | null;
  completedAt?: string | null;
  completedBy?: string | null;
  tenantId: string;
  createdAt: string;
  createdBy: string;
}

export interface WorkflowInstanceHistoryDto {
  id: string;
  fromStateId: string;
  toStateId: string;
  actionName: string;
  transitionedBy: string;
  comment?: string | null;
  transitionedAt: string;
}

export interface WorkflowInstanceDto {
  id: string;
  definitionId: string;
  entityType: string;
  entityId: string;
  currentStateId: string;
  tenantId: string;
  startedAt: string;
  completedAt?: string | null;
  dueAt?: string | null;
  currentStateEnteredAt: string;
  history: WorkflowInstanceHistoryDto[];
}

export interface ExecuteTransitionRequest {
  transitionId: string;
  actorRoleCode?: string | null;
  comment?: string | null;
}

export interface ReassignTaskRequest {
  newAssigneeId: string;
  reason?: string | null;
}
