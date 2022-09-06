-- System operator is required in order to create default charges which we want to seed in the database.
-- Hardcode ID because hopefully the market participant registry will pick this ID up when adding system operator.
INSERT INTO [Charges].[MarketParticipant] VALUES ('af450c03-1937-4ea1-bb66-17b6e4aa51f5','5790000432752', 3, 1);
