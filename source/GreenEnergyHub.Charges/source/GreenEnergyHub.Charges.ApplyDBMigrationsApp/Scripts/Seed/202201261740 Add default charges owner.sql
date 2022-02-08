-- System operator is required in order to create default charges which we want to seed in the database.
-- Hardcode ID because hopefully the market participant registry will pick this ID up when adding system operator.
INSERT INTO [Charges].[MarketParticipant] VALUES ('AF450C03-1937-4EA1-BB66-17B6E4AA51F5','5790000432752', 3, 1);
