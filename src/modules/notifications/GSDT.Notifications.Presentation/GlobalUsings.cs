// Global usings for GSDT.Notifications.Presentation

global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.RateLimiting;

global using GSDT.SharedKernel.Application;
global using GSDT.SharedKernel.Compliance;
global using GSDT.SharedKernel.Contracts;
global using GSDT.SharedKernel.Presentation;

global using GSDT.Notifications.Application.Commands.SendNotification;
global using GSDT.Notifications.Application.Commands.MarkNotificationRead;
global using GSDT.Notifications.Application.Commands.MarkAllNotificationsRead;
global using GSDT.Notifications.Application.Commands.CreateNotificationTemplate;
global using GSDT.Notifications.Application.Commands.UpdateNotificationTemplate;
global using GSDT.Notifications.Application.Commands.DeleteNotificationTemplate;
global using GSDT.Notifications.Application.Queries.GetUserNotifications;
global using GSDT.Notifications.Application.Queries.GetUnreadNotificationCount;
global using GSDT.Notifications.Application.Queries.GetNotificationTemplates;
global using GSDT.Notifications.Application.Queries.GetNotificationTemplateById;
