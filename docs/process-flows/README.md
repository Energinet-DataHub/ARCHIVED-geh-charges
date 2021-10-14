# Process flows - Charges domain

All process flows within the Charges domain will be gathered here.

| Process flows |
|-------------------|
|[Change charge price list](#Change-charge-price-list)|
|[Persist charge](#Persist-charge)|
|[Create charge link](#Create-charge-link)|
<br>

## Change Charge Price List

The following process flow depicts the different paths a change of charge price list can take through the domain.
It also shows the micro services involved along with the activities they perform.

![Process flow](images/ChargePriceListProcessFlow.png)

## Persist Charge

Below process flow depicts the rule set for persisting an incoming charge in the SQL database.
The rule set documents the different persistence paths the system can take given circumstances like charge already exist (same Charge ID and Owner) and if so, whether the incoming charge has a period that overlaps any of the existing charge's periods.
All paths outline the exact database tables it needs to interact with and when existing charge data, e.g periods or prices need to be retired.

The rule set assumes the incoming charge has been converted to an internal model, i.e. the Charge Command, and it has passed both input and business validations.

![Persist charge](images/PersistingChargesRuleSet_ProcessFlow.png)

## Create Charge Link

The following image depicts the process flow of creating charge links in the domain.
It also shows the micro services involved along with the activities they perform.

![Create charge link](images/CreateChargeLinkProcessFlow.png)
