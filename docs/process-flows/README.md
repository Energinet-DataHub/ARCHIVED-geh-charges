# Process flows - Charges domain

All process flows within the Charges domain will be gathered here.

| Process flows |
|-------------------|
|[Charge Price List Flow](#Charge-Price-List-Flow)|
|[Create Charge Link Flow](#Create-Charge-Link-Flow)|
<br>

## Charge Price List Flow

The following image depicts the charge price list flow.
It also shows the micro services involved along with the activities they perform.

![Charge flow](images/ChargePriceListProcessFlow.png)

### Persist Charge and Charge Prices

OBS: This rule set is pending complete implementation.

Below process flow depicts the rule set used by the `ChargeCommandReceiverEndpoint` for persisting an incoming charge and prices in the SQL database.
The rule set documents the different persistence paths the system can take given circumstances like charge already exist (same Charge ID and Owner) and if so, whether the incoming charge has a period that overlaps any of the existing charge's periods.
All paths outline the exact database tables it needs to interact with and when existing charge data, e.g periods or prices need to be retired.

The rule set assumes the incoming charge has been converted to an internal model, i.e. the Charge Command, and it has passed both input and business validations.

![Persist charge](images/PersistingChargesRuleSet_ProcessFlow.png)

## Charge Link Flow

The following image depicts the charge link process flow.
It also shows the micro services involved along with the activities they perform.

![Charge link flow](images/CreateChargeLinkProcessFlow.png)
