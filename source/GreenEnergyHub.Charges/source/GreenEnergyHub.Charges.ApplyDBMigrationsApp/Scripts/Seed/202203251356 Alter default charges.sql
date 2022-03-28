/* Charge 40000 */
update chp
set chp.[Name] = 'Transmissions nettarif',
    chp.[Description] = 'Netafgiften, for både forbrugere og producenter, dækker omkostninger til drift og vedligehold af det overordnede elnet (132/150 og 400 kv nettet) og drift og vedligehold af udlandsforbindelserne'
from [Charges].[ChargePeriod] as chp
    inner join [Charges].[Charge] as chg
    on chg.id = chp.ChargeId
where chg.SenderProvidedChargeId = '40000'
  and chg.[Type] = 3
  and chg.[OwnerId] = (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')

/* Charge 40010 */
update chp
set chp.[Name] = 'Indfødningstarif produktion',
    chp.[Description] = 'Netafgiften, for både forbrugere og producenter, dækker omkostninger til drift og vedligehold af det overordnede elnet (132/150 og 400 kv nettet) og drift og vedligehold af udlandsforbindelserne.'
from [Charges].[ChargePeriod] as chp
    inner join [Charges].[Charge] as chg
    on chg.id = chp.ChargeId
where chg.SenderProvidedChargeId = '40010'
    and chg.[Type] = 3
    and chg.[OwnerId] = (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')

/* Charge 41000 */
update chp
set chp.[Name] = 'Systemtarif',
    chp.[Description] = 'Systemafgiften dækker omkostninger til forsyningssikkerhed og elforsyningens kvalitet.'
from [Charges].[ChargePeriod] as chp
    inner join [Charges].[Charge] as chg
    on chg.id = chp.ChargeId
where chg.SenderProvidedChargeId = '41000'
    and chg.[Type] = 3
    and chg.[OwnerId] = (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')

/* EA-003 */
update chp
set chp.[Name] = 'Negativt reduceret elafgift',
    chp.[Description] = 'Negativ reduceret elafgift for elvarmekunder til brug for modregning af den almindelige reducerede elafgift for elvarmekunder. Bruges kun sammen med D14 målepunkter.'
from [Charges].[ChargePeriod] as chp
    inner join [Charges].[Charge] as chg
    on chg.id = chp.ChargeId
where chg.SenderProvidedChargeId = 'EA-003'
  and chg.[Type] = 3
  and chg.[OwnerId] = (SELECT Id FROM [Charges].[MarketParticipant] WHERE MarketParticipantId = '5790000432752')
