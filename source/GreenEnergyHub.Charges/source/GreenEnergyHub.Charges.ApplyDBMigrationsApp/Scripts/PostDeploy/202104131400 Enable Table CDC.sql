EXEC sys.sp_cdc_enable_table  
@source_schema = N'dbo',
@source_name   = N'ChargeType',
@role_name = NULL
GO  

EXEC sys.sp_cdc_enable_table  
@source_schema = N'dbo',
@source_name   = N'ResolutionType',
@role_name = NULL
GO

EXEC sys.sp_cdc_enable_table  
@source_schema = N'dbo',
@source_name   = N'VATPayerType',
@role_name = NULL
GO

EXEC sys.sp_cdc_enable_table  
@source_schema = N'dbo',
@source_name   = N'MarketParticipant',
@role_name = NULL
GO

EXEC sys.sp_cdc_enable_table  
@source_schema = N'dbo',
@source_name   = N'Charge',
@role_name = NULL
GO

EXEC sys.sp_cdc_enable_table  
@source_schema = N'dbo',
@source_name   = N'ChargePrice',
@role_name = NULL
GO

EXEC sys.sp_cdc_enable_table  
@source_schema = N'dbo',
@source_name   = N'ValidationRuleConfiguration',
@role_name = NULL
GO
