------------------------------------------------------------------------------------------------------------------------
-- Rename column
------------------------------------------------------------------------------------------------------------------------

EXEC SP_RENAME 'Charges.OutboxMessage.CorrelationTraceContext', 'Charges.OutboxMessage.OutboxMessage.CorrelationId', 'COLUMN'