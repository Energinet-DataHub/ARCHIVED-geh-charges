# Change of charges validations

The following validation rules are currently implemented in the charge domain when changing charges.

|**Rule number**|**Description**|**Rejection code**|**Charge Types**|
|:-|:-|:-|:-|
|VR.009|The energy business process of a metering point is mandatory|D02|All|
|VR.150|The sender of a message is mandatory|D02|All|
|VR.152*|The sender of a message must currently be an existing and active market party (company)|D02|All|
|VR.153|The recipient of a message is mandatory|D02|All|
|VR.209|The data information must be received within the correct time period|E17|All|
|VR.223|The identification of a transaction is mandatory|E0H|All|
|VR.404|The document type of a message must be D10 (Request update charge information)|D02|All|
|VR.424|The energy business process of a message must be D18 (Update charge information)|D02|All|
|VR.440|The identification of a charge is mandatory|E0H|All|
|VR.441|The identification of a charge consists of maximal 10 characters|E86|All|
|VR.446|The name of a charge consists of maximal 50 characters|E86|All|
|VR.447|The description of a charge consists of maximal 2048 characters|E86|All|
|VR.449|The type of a charge has domain values D01 (Subscription), D02 (Fee), D03 (Tariff)|E86|All|
|VR.457|The energy price of a charge consists of maximal 14 digits with format 99999999.999999|E86|All|
|VR.488|The VAT class of a charge has domain values D01 (No VAT), D02 (VAT)|E86|All|
|VR.505-1|The Tariff to which the charge information applies must have period type Day, Hour or Quarter of Hour|D23|Tariff|
|VR.505-2|The Fee to which the charge information applies must have period type Day|D23|Fee|
|VR.505-3|The Subscription to which the charge information applies must have period type Month|D23|Subscription|
|VR.507-1|The Tariff to which the charge information applies must have 1 price for period type Day, 24 prices for period type Hour or 96 prices for period type Quarter of Hour|E87|Tariff|
|VR.507-2|The Fee/Subscription to which the charge information applies must have 1 price|E87|Fee, Subscription|
|VR.509|The charge price must be plausible (i.e. value less than 1.000.000)|E90|All|
|VR.531|The occurrence of a charge is mandatory|E0H|All|
|VR.532|The owner of a charge is mandatory|E0H|All|
|VR.630|The VAT entitlement for a charge cannot be updated|D14|Tariff|
|VR.902*|Update and stops are not implemented yet|D13|All|
|VR.903|The Tax indicator for a charge cannot be updated|D14|Tariff|

* VR.152 is not fully implemented. Right now we only validate that it is filled with something
* VR.902 is temporary and will be changed once update and stop functionality has been implemented.
