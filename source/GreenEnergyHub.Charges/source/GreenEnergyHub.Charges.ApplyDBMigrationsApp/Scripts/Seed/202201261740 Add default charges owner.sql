-- System operator is required in order to create default charges which we want to seed in the database.
-- Hardcode ID because hopefully the actor register will pick this ID up when adding system operator.
INSERT INTO [Charges].[MarketParticipant] VALUES ('25995e18-29f5-4f9b-b644-0b5dd876f30a','5790000432752', 3, 1);
