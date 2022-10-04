# Charges asynchronous validations

The following asynchronous validation rules are currently implemented in the charge domain.

|**Rule number**|**Description**|**Rejection code**|**Charge Information (D18)**|**Charge Prices (D08)**|**Charge Links**|
|:-|:-|:-|:-|:-|:-|
|VR.150|The sender of a message is mandatory|D02|✓|✓||
|VR.152*|The sender of a message must currently be an existing and active market party (company)|D02|✓|✓|
|VR.153|The recipient of a message is mandatory|D02|✓|✓||
|VR.165|The recipient role of a message must be DDZ (Metering point administrator or DataHub)|E55|✓|✓||
|VR.200|Metering point do not exist|E10|||✓|
|VR.209|The data received must be within the correct time period|E17|✓|✓||
|VR.223|The identification of a transaction is mandatory|E0H|✓|✓||
|VR.404|The document type of a message must be D10 (Request update charge information)|D02|✓|✓||
|VR.424|The energy business process of a message must be D08 (Update Charge Prices) or D18 (Update charge information)|D02|✓|✓||
|VR.440|The identification of a charge is mandatory|E0H|✓|✓||
|VR.441|The identification of a charge consists of maximal 10 characters|E86|✓|✓||
|VR.446|The name of a charge consists of maximal 132 characters|E86|✓|||
|VR.447|The description of a charge consists of maximal 2048 characters|E86|✓|||
|VR.449|The type of a charge has domain values D01 (Subscription), D02 (Fee), D03 (Tariff)|E86|✓|✓||
|VR.457|The energy price of a charge consists of maximal 14 digits with format 99999999.999999|E86||✓||
|VR.488|The VAT class of a charge has domain values D01 (No VAT), D02 (VAT)|E86|✓|||
|VR.505-1|Tariff must have period type Day, Hour or Quarter of Hour|D23|(✓) Tariff only|(✓) Tariff only||
|VR.505-2|Fee must have period type Month|D23|(✓) Fee only|(✓) Fee only||
|VR.505-3|Subscription must have period type Month|D23|(✓) Subscription only|(✓) Subscription only||
|VR.507-1|The Tariff to which the charge price applies must have 1 price for period type Day, 24 prices for period type Hour or 96 prices for period type Quarter of Hour|E87||(✓) Tariff only||
|VR.508|Only System Operator role is allowed to submit requests concerning tax tariffs|E0I|(✓) Tariff only|(✓) Tariff only||
|VR.509|The charge price must be plausible (i.e. value less than 1.000.000)|E90||✓||
|VR.513|Charge owner must match sender of a message|E0I|✓|✓||
|VR.531|The occurrence of a charge is mandatory|E0H|✓|✓||
|VR.532|The owner of a charge is mandatory|E0H|✓|✓||
|VR.679*|Charge do not exist|E0I|||✓|
|VR.902*|Update and stop charge link is not yet implemented|D13|||✓|
|VR.903|The Tax indicator for a charge cannot be updated|D14|(✓) Tariff only|||
|VR.904|TransparentInvoicing must be false for charges of type Fee (D02).|D67|(✓) Fee only|||
|VR.905|As gaps are not allowed in a charge timeline, e.g. when a charge is stopped, any attempts to update it with an effective date after the stop date, must be rejected.|D14|✓|||
|VR.906|Transaction not completed: The request received contained multiple transactions for the same charge, and one of the previous transactions failed validation why this transaction is also rejected|D14|✓|✓||
|VR.907|After creating a Charge, it is no longer allowed to change the resolution of that charge|D23|✓|||
|VR.909|The number of prices received does not match the expected number of prices given the time interval and resolution provided.|E87|✓|✓||
|VR.910|Charge name is mandatory|E0H|✓|||
|VR.911|Charge description is mandatory|E0H|✓|||
|VR.912|Resolution is mandatory|E0H|✓|✓||
|VR.915|Transparent invoicing is mandatory|E0H|✓|||
|VR.916|Tax indicator is mandatory|E0H|✓|||
|VR.917|Termination date and effective date must have the same value|E0H|✓|||
|VR.919|The time interval (start and end) of the price series must equal midnight local time, expressed in UTC+0.|E86|✓|✓||
|VR.920|Tax indicator must be set to false for a subscription|D14|✓|||
|VR.921|Tax indicator must be set to false for a fee|D14|✓|||
|VR.922|The identification of a charge operation consists of maximal 36 characters|E86|✓|✓||
|VR.923|The monthly price series' END date time must be 1st of month, unless it matches the Charge's STOP date.|E86||✓||

* VR.152 is not fully implemented. Right now we only validate that it is filled with something
* VR.679 is not fully implemented. For now it verifies that the charge exist, not checking that the linked period is within the charge's validity period
* VR.902 is temporary and will be changed once update and stop functionality has been implemented.
