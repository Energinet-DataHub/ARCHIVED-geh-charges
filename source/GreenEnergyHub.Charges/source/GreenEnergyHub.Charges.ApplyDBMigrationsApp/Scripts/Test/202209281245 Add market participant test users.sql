-- Add market participant with role Energy Supplier DDQ with status new
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8100000001001', 1, 1, NEWID(), NEWID());
-- Add market participant with role Energy Supplier DDQ with status inactive
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8100000001003', 1, 3, NEWID(), NEWID());
-- Add market participant with role Energy Supplier DDQ with status passive
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8100000001004', 1, 4, NEWID(), NEWID());
-- Add market participant with role Energy Supplier DDQ with status deleted
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8100000001005', 1, 5, NEWID(), NEWID());

-- Add market participant with role Grid Access Provider DDM with status new
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8100000002001', 2, 1, NEWID(), NEWID());
-- Add market participant with role Grid Access Provider DDM with status inactive
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8100000002003', 2, 3, NEWID(), NEWID());
-- Add market participant with role Grid Access Provider DDM with status passive
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8100000002004', 2, 4, NEWID(), NEWID());
-- Add market participant with role Grid Access Provider DDM with status deleted
INSERT INTO [Charges].[MarketParticipant] VALUES (NEWID(),'8100000002005', 2, 5, NEWID(), NEWID());