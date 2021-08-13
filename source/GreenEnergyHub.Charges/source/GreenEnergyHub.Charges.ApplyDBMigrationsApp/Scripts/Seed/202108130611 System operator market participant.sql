BEGIN /* We need to be careful here, the participant we are adding might already be on the server if it the server includes test data */
  IF NOT EXISTS (SELECT MarketParticipantId FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
  BEGIN
    INSERT INTO [Charges].[MarketParticipant] (MarketParticipantId, Name, Role) VALUES ('5790000432752', 'System Operator', 3); /* System Operator is used as a name for this participant, it is not by mistake */
  END
END
GO