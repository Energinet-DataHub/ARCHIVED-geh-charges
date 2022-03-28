/* Charge 40000 */
update [Charges].[Charge]
set [Name] = 'Transmissions nettarif',
    [Description] = 'Netafgiften, for både forbrugere og producenter, dækker omkostninger til drift og vedligehold af det overordnede elnet (132/150 og 400 kv nettet) og drift og vedligehold af udlandsforbindelserne'
where SenderProvidedChargeId = '40000'
  and [Type] = 3
  and [OwnerId] = (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')

/* Charge 40010 */
update [Charges].[Charge]
set [Name] = 'Indfødningstarif produktion',
    [Description] = 'Netafgiften, for både forbrugere og producenter, dækker omkostninger til drift og vedligehold af det overordnede elnet (132/150 og 400 kv nettet) og drift og vedligehold af udlandsforbindelserne.'
where SenderProvidedChargeId = '40010'
  and [Type] = 3
  and [OwnerId] = (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')

/* Charge 41000 */
update [Charges].[Charge]
set [Name] = 'Systemtarif',
    [Description] = 'Systemafgiften dækker omkostninger til forsyningssikkerhed og elforsyningens kvalitet.'
where SenderProvidedChargeId = '41000'
  and [Type] = 3
  and [OwnerId] = (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')

/* EA-003 */
update [Charges].[Charge]
set [Name] = 'Negativt reduceret elafgift',
    [Description] = 'Negativ reduceret elafgift for elvarmekunder til brug for modregning af den almindelige reducerede elafgift for elvarmekunder. Bruges kun sammen med D14 målepunkter.'
where SenderProvidedChargeId = 'EA-003'
  and [Type] = 3
  and [OwnerId] = (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
