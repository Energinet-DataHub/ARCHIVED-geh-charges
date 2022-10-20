------------------------------------------------------------------------------------------------------------------------
-- DATA CLEAN-UP:
-- FIND CHARGES WITHOUT ANY CHARGE PERIODS AND CREATE A CHARGE PERIOD THAT STARTS AND ENDS 2022-12-31 23:00:00
------------------------------------------------------------------------------------------------------------------------

BEGIN TRAN

INSERT INTO Charges.ChargePeriod
SELECT NEWID(), chg.Id, 0, 'Description', 'Name', 0, '2022-12-31 23:00:00', '2022-12-31 23:00:00'
FROM Charges.Charge AS chg
LEFT JOIN Charges.ChargePeriod AS cp
ON chg.Id = cp.ChargeId
WHERE cp.ChargeId is null;

COMMIT;