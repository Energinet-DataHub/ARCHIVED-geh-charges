-- Transitioning to using the shared actor register we reset all seeded test data in all environments
-- in order to avoid building on or publishing data that does not exist or is meaningless to other domains

DELETE FROM Charges.ChargeLink

DELETE FROM Charges.MeteringPoint

DELETE FROM Charges.ChargePoint
DELETE FROM Charges.Charge

DELETE FROM Charges.MarketParticipant

DELETE FROM MessageHub.AvailableChargeDataPoints
DELETE FROM MessageHub.AvailableChargeData

DELETE FROM MessageHub.AvailableChargeLinksData

DELETE FROM MessageHub.AvailableChargeLinksReceiptValidationError
DELETE FROM MessageHub.AvailableChargeLinksReceiptData

DELETE FROM MessageHub.AvailableChargeReceiptValidationError
DELETE FROM MessageHub.AvailableChargeReceiptData
