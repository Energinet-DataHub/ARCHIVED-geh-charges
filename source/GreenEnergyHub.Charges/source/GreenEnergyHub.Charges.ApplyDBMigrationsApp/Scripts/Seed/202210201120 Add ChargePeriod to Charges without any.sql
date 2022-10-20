------------------------------------------------------------------------------------------------------------------------
-- DATA CLEAN-UP:
-- FIND CHARGES WITHOUT ANY CHARGE PERIODS
-- FOR EACH: CREATE A CHARGE PERIOD THAT STARTS AND ENDS 2022-12-31 23:00:00
------------------------------------------------------------------------------------------------------------------------

BEGIN TRAN
CREATE TABLE #temp (
    RowId INT IDENTITY(1,1) not null,
    Id UNIQUEIDENTIFIER);

-- Insert Charge ID's without a charge period into #temp
INSERT INTO #temp
SELECT c.Id
FROM Charges.Charge AS c
LEFT JOIN Charges.ChargePeriod AS cp
ON c.Id = cp.ChargeId
WHERE cp.ChargeId is null;

DECLARE @currentRowId INT;
SET @currentRowId = 1

DECLARE @RowCount INT;
SET @RowCount = (SELECT COUNT(*) FROM #temp);

DECLARE @chargeId NVARCHAR(36)

WHILE(@currentRowId <= @RowCount)
    BEGIN
		SET @chargeId = (SELECT Id FROM #temp where RowId = @currentRowId);
        INSERT INTO Charges.ChargePeriod VALUES (NEWID(), @chargeId, 0, 'Description', 'Name', 0, '2022-12-31 23:00:00', '2022-12-31 23:00:00')
        SET @currentRowId += 1;
    END
DROP TABLE #temp;
COMMIT;