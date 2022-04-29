------------------------------------------------------------------------------------------------------------------------
-- Drop constraints to marketparticpant table, allowing creation of gridareas without a marketparticipant
------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [Charges].[GridArea]
DROP CONSTRAINT FK_GridArea_MarketParticipant;