INSERT INTO [dbo].[ChargeType] VALUES (1, 'D01', 'Subscription');
INSERT INTO [dbo].[ChargeType] VALUES (2, 'D02', 'Fee');
INSERT INTO [dbo].[ChargeType] VALUES (3, 'D03', 'Tariff');
GO

INSERT INTO [dbo].[ResolutionType] VALUES (1, 'PT15M');
INSERT INTO [dbo].[ResolutionType] VALUES (2, 'PT1H');
INSERT INTO [dbo].[ResolutionType] VALUES (3, 'P1D');
INSERT INTO [dbo].[ResolutionType] VALUES (4, 'P1M');
GO

INSERT INTO [dbo].[VATPayerType] VALUES (1, 'D01');
INSERT INTO [dbo].[VATPayerType] VALUES (2, 'D02');
GO

INSERT INTO [dbo].[ValidationRuleConfiguration] VALUES ('VR209_Start_Of_Valid_Interval_From_Now_In_Days', '31');
INSERT INTO [dbo].[ValidationRuleConfiguration] VALUES ('VR209_End_Of_Valid_Interval_From_Now_In_Days', '1095');
GO