CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "users" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_users" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Email" TEXT NOT NULL
);

CREATE UNIQUE INDEX "IX_users_Email" ON "users" ("Email");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260203123053_InitialUsers', '10.0.2');

COMMIT;

BEGIN TRANSACTION;
ALTER TABLE "users" ADD "PasswordHash" TEXT NULL;

ALTER TABLE "users" ADD "SecurityStamp" TEXT NULL;

CREATE TABLE "permissions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_permissions" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL
);

CREATE TABLE "refresh_tokens" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_refresh_tokens" PRIMARY KEY,
    "Token" TEXT NOT NULL,
    "ExpiryDate" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL,
    "CreatedByIp" TEXT NOT NULL,
    "DeviceInfo" TEXT NOT NULL,
    "RevokedDate" TEXT NULL,
    "RevokedByIp" TEXT NULL,
    "ReasonRevoked" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "FK_refresh_tokens_users_UserId" FOREIGN KEY ("UserId") REFERENCES "users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "roles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_roles" PRIMARY KEY,
    "Name" TEXT NOT NULL
);

CREATE TABLE "user_logins" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_user_logins" PRIMARY KEY,
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "FK_user_logins_users_UserId" FOREIGN KEY ("UserId") REFERENCES "users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "role_permissions" (
    "PermissionsId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_role_permissions" PRIMARY KEY ("PermissionsId", "RoleId"),
    CONSTRAINT "FK_role_permissions_permissions_PermissionsId" FOREIGN KEY ("PermissionsId") REFERENCES "permissions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_role_permissions_roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "roles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "user_roles" (
    "RolesId" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_user_roles" PRIMARY KEY ("RolesId", "UserId"),
    CONSTRAINT "FK_user_roles_roles_RolesId" FOREIGN KEY ("RolesId") REFERENCES "roles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_user_roles_users_UserId" FOREIGN KEY ("UserId") REFERENCES "users" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_permissions_Name" ON "permissions" ("Name");

CREATE UNIQUE INDEX "IX_refresh_tokens_Token" ON "refresh_tokens" ("Token");

CREATE INDEX "IX_refresh_tokens_UserId" ON "refresh_tokens" ("UserId");

CREATE INDEX "IX_role_permissions_RoleId" ON "role_permissions" ("RoleId");

CREATE UNIQUE INDEX "IX_roles_Name" ON "roles" ("Name");

CREATE UNIQUE INDEX "IX_user_logins_LoginProvider_ProviderKey" ON "user_logins" ("LoginProvider", "ProviderKey");

CREATE INDEX "IX_user_logins_UserId" ON "user_logins" ("UserId");

CREATE INDEX "IX_user_roles_UserId" ON "user_roles" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260210173820_AddAuthSystem', '10.0.2');

COMMIT;

BEGIN TRANSACTION;
CREATE TABLE "activities" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_activities" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "CategoryId" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "AllowOverlap" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL
);

CREATE TABLE "activity_log_values" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_activity_log_values" PRIMARY KEY,
    "ActivityLogId" TEXT NOT NULL,
    "ValueDefinitionId" TEXT NOT NULL,
    "Value" TEXT NOT NULL
);

CREATE TABLE "activity_logs" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_activity_logs" PRIMARY KEY,
    "ActivityId" TEXT NOT NULL,
    "StartedAt" TEXT NOT NULL,
    "EndedAt" TEXT NULL
);

CREATE TABLE "activity_value_definitions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_activity_value_definitions" PRIMARY KEY,
    "ActivityId" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "ValueType" TEXT NOT NULL,
    "IsRequired" INTEGER NOT NULL,
    "Unit" TEXT NULL,
    "MinValue" TEXT NULL,
    "MaxValue" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL
);

CREATE TABLE "categories" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_categories" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL
);

CREATE TABLE "user_sessions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_user_sessions" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "SessionToken" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "ExpiresAt" TEXT NOT NULL,
    "IsActive" INTEGER NOT NULL
);

CREATE TABLE "UserRoles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_UserRoles" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "FK_UserRoles_roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "roles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserRoles_users_UserId" FOREIGN KEY ("UserId") REFERENCES "users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_UserRoles_RoleId" ON "UserRoles" ("RoleId");

CREATE INDEX "IX_UserRoles_UserId" ON "UserRoles" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260214163003_SyncModelWithEntities', '10.0.2');

COMMIT;

BEGIN TRANSACTION;
CREATE INDEX "IX_activity_log_values_ActivityLogId" ON "activity_log_values" ("ActivityLogId");

CREATE INDEX "IX_activity_log_values_ValueDefinitionId" ON "activity_log_values" ("ValueDefinitionId");

CREATE TABLE "ef_temp_activity_log_values" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_activity_log_values" PRIMARY KEY,
    "ActivityLogId" TEXT NOT NULL,
    "Value" TEXT NOT NULL,
    "ValueDefinitionId" TEXT NOT NULL,
    CONSTRAINT "FK_activity_log_values_activity_logs_ActivityLogId" FOREIGN KEY ("ActivityLogId") REFERENCES "activity_logs" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_activity_log_values_activity_value_definitions_ValueDefinitionId" FOREIGN KEY ("ValueDefinitionId") REFERENCES "activity_value_definitions" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_activity_log_values" ("Id", "ActivityLogId", "Value", "ValueDefinitionId")
SELECT "Id", "ActivityLogId", "Value", "ValueDefinitionId"
FROM "activity_log_values";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "activity_log_values";

ALTER TABLE "ef_temp_activity_log_values" RENAME TO "activity_log_values";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_activity_log_values_ActivityLogId" ON "activity_log_values" ("ActivityLogId");

CREATE INDEX "IX_activity_log_values_ValueDefinitionId" ON "activity_log_values" ("ValueDefinitionId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260223021715_SyncModelWithChanges', '10.0.2');

BEGIN TRANSACTION;
ALTER TABLE "user_sessions" ADD "DeviceInfo" TEXT NOT NULL DEFAULT '';

ALTER TABLE "user_sessions" ADD "IpAddress" TEXT NOT NULL DEFAULT '';

ALTER TABLE "user_sessions" ADD "Location" TEXT NULL;

ALTER TABLE "user_sessions" ADD "RevokedAt" TEXT NULL;

ALTER TABLE "user_sessions" ADD "UserAgent" TEXT NULL;

ALTER TABLE "categories" ADD "Color" TEXT NOT NULL DEFAULT '';

ALTER TABLE "activities" ADD "Color" TEXT NOT NULL DEFAULT '';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260331045315_AddUserSessionDetails', '10.0.2');

COMMIT;

