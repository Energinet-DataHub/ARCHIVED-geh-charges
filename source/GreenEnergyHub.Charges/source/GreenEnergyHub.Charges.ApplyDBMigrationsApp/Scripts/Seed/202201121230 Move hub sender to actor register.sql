BEGIN /* We need to be careful here, the participant we are adding might already be on the server if the server includes test data */
  IF NOT EXISTS (SELECT MarketParticipantId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790001330552')
BEGIN
INSERT INTO [Charges].[MarketParticipant] (Id, MarketParticipantId, Roles, IsActive) VALUES (NEWID(), '5790001330552', 7, 1);
END
END
GO
