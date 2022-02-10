# Process flows - Charges domain

All process flows within the Charges domain will be gathered here.

| Process flows |
|-------------------|
|<b>[Charge Price List Flow](#Charge-Price-List-Flow)</b>|
|   - [Persist Charge](#Persist-Charge)|
|   - [Persist Charge Prices](#Persist-Charge-Prices)|
|<b>[Charge Link Flow](#Charge-Link-Flow)<b>|
<br>

## Charge Price List Flow

The following image depicts the charge price list flow.
It also shows the micro services involved along with the activities they perform.

![Charge flow](images/ChargePriceListProcessFlow.png)

### Persist Charge

The below process flow depicts the rule set applied in the `ChargeCommandReceiverEndpoint` for persisting incoming charges in the SQL database.  
It documents the different persistence paths the system takes given circumstances like charge already exists (same Charge ID, type and owner), whether it is an update or stop operation, and whether a stop already exists on the charge's timeline and if this stop is being cancelled or not.

The rule set was built upon the scenarios listed [here](images/PersistingCharges_Update_And_Stop_MasterData_Examples.png) and it is assumed that the incoming charge has been converted to an internal model, i.e. the Charge Command and has passed both input and business validation.

Note, stopping a charge results in a removal of any registered prices from the stop date and forwards.

![Persist charge](images/PersistingChargesRuleSet_ProcessFlow.png)
<br>

### Persist Charge Prices

The rule set for persisting charge prices is yet to be defined. Pending business reason code decision.
<br>

## Charge Link Flow

The following image depicts the charge link process flow.
It also shows the micro services involved along with the activities they perform.

![Charge link flow](images/CreateChargeLinkProcessFlow.png)
