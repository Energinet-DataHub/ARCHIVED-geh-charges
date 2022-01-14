-- Create SenderId and SenderRole columns

ALTER TABLE [MessageHub].[AvailableChargeData]
ADD [SenderId] NVARCHAR(35);

ALTER TABLE [MessageHub].[AvailableChargeData]
ADD [SenderRole] INT;

ALTER TABLE [MessageHub].[AvailableChargeLinksReceiptData]
ADD [SenderId] NVARCHAR(35);

ALTER TABLE [MessageHub].[AvailableChargeLinksReceiptData]
ADD [SenderRole] INT;

ALTER TABLE [MessageHub].[AvailableChargeLinksData]
ADD [SenderId] NVARCHAR(35);

ALTER TABLE [MessageHub].[AvailableChargeLinksData]
ADD [SenderRole] INT;

ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
ADD [SenderId] NVARCHAR(35);

ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
ADD [SenderRole] INT;

GO

-- Set column values

DECLARE @data_hub NVARCHAR(35) = '5790001330552'
DECLARE @metering_point_administrator_role INT = 7

UPDATE [MessageHub].[AvailableChargeData]
SET SenderId = @data_hub 
WHERE SenderId IS NULL

UPDATE [MessageHub].[AvailableChargeData]
SET SenderRole = @metering_point_administrator_role
WHERE SenderRole IS NULL

UPDATE [MessageHub].[AvailableChargeLinksReceiptData]
SET SenderId = @data_hub
WHERE SenderId IS NULL

UPDATE [MessageHub].[AvailableChargeLinksReceiptData]
SET SenderRole = @metering_point_administrator_role
WHERE SenderRole IS NULL

UPDATE [MessageHub].[AvailableChargeLinksData]
SET SenderId = @data_hub
WHERE SenderId IS NULL

UPDATE [MessageHub].[AvailableChargeLinksData]
SET SenderRole = @metering_point_administrator_role
WHERE SenderRole IS NULL

UPDATE [MessageHub].[AvailableChargeReceiptData]
SET SenderId = @data_hub
WHERE SenderId IS NULL

UPDATE [MessageHub].[AvailableChargeReceiptData]
SET SenderRole = @metering_point_administrator_role
WHERE SenderRole IS NULL

GO

-- Enable NOT NULL constraints

ALTER TABLE [MessageHub].[AvailableChargeData]
ALTER COLUMN SenderId NVARCHAR(35) NOT NULL;

ALTER TABLE [MessageHub].[AvailableChargeData]
ALTER COLUMN SenderRole INT NOT NULL;

ALTER TABLE [MessageHub].[AvailableChargeLinksReceiptData]
ALTER COLUMN SenderId NVARCHAR(35) NOT NULL;

ALTER TABLE [MessageHub].[AvailableChargeLinksReceiptData]
ALTER COLUMN SenderRole INT NOT NULL;

ALTER TABLE [MessageHub].[AvailableChargeLinksData]
ALTER COLUMN SenderId NVARCHAR(35) NOT NULL;

ALTER TABLE [MessageHub].[AvailableChargeLinksData]
ALTER COLUMN SenderRole INT NOT NULL;

ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
ALTER COLUMN SenderId NVARCHAR(35) NOT NULL;

ALTER TABLE [MessageHub].[AvailableChargeReceiptData]
ALTER COLUMN SenderRole INT NOT NULL;
