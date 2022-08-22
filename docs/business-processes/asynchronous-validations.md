# Charges asynchronous validations

The following asynchronous validation rules are currently implemented in the charge domain.

|**Rule number**|**Description**|**Rejection code**|**Charge Types**|**Charge Links**|
|:-|:-|:-|:-|:-|
|VR.150|The sender of a message is mandatory|D02|All|N/A|
|VR.152*|The sender of a message must currently be an existing and active market party (company)|D02|All|N/A|
|VR.153|The recipient of a message is mandatory|D02|All|N/A|
|VR.167|The recipient role of a message must be DDZ (Metering point administrator or DataHub)|E55|All|N/A|
|VR.200|Metering point do not exist|E10|N/A|X|
|VR.209|The data information must be received within the correct time period|E17|All|N/A|
|VR.223|The identification of a transaction is mandatory|E0H|All|N/A|
|VR.404|The document type of a message must be D10 (Request update charge information)|D02|All|N/A|
|VR.424|The energy business process of a message must be D08 (Update Charge Prices) or D18 (Update charge information)|D02|All|N/A|
|VR.440|The identification of a charge is mandatory|E0H|All|N/A|
|VR.441|The identification of a charge consists of maximal 10 characters|E86|All|N/A|
|VR.446|The name of a charge consists of maximal 132 characters|E86|All|N/A|
|VR.447|The description of a charge consists of maximal 2048 characters|E86|All|N/A|
|VR.449|The type of a charge has domain values D01 (Subscription), D02 (Fee), D03 (Tariff)|E86|All|N/A|
|VR.457|The energy price of a charge consists of maximal 14 digits with format 99999999.999999|E86|All|N/A|
|VR.488|The VAT class of a charge has domain values D01 (No VAT), D02 (VAT)|E86|All|N/A|
|VR.505-1|The Tariff to which the charge information applies must have period type Day, Hour or Quarter of Hour|D23|Tariff|N/A|
|VR.505-2|The Fee to which the charge information applies must have period type Month|D23|Fee|N/A|
|VR.505-3|The Subscription to which the charge information applies must have period type Month|D23|Subscription|N/A|
|VR.507-1|The Tariff to which the charge information applies must have 1 price for period type Day, 24 prices for period type Hour or 96 prices for period type Quarter of Hour|E87|Tariff|N/A|
|VR.509|The charge price must be plausible (i.e. value less than 1.000.000)|E90|All|N/A|
|VR.513|Charge owner must match sender of a message|E0I|All|N/A|
|VR.531|The occurrence of a charge is mandatory|E0H|All|N/A|
|VR.532|The owner of a charge is mandatory|E0H|All|N/A|
|VR.679*|Charge do not exist|E0I|N/A|X|
|VR.902-1*|Stop charge is not yet implemented|D13|All|N/A|
|VR.902-2*|Update and stop charge link is not yet implemented|D13|N/A|X|
|VR.903|The Tax indicator for a charge cannot be updated|D14|Tariff|N/A|
|VR.904|TransparentInvoicing must be false for charges of type Fee (D02).|D67|Fee|N/A|
|VR.905|As gaps are not allowed in a Charge timeline, e.g. when a charge is stopped, any attempts to update it with an effective date after the stop date, must be rejected.|D14|All|N/A|
|VR.906|Transaction not completed: The request received contained multiple transactions for the same charge, and one of the previous transactions failed validation why this transaction is also rejected|D14|All|N/A|
|VR.907|After creating a Charge, it is no longer allowed to change the resolution of that charge|D23|All|N/A|
|VR.909|The number of prices received does not match the expected number of prices given the time interval and resolution provided.|E87|All|N/A|
|VR.910|Charge name is mandatory|E0H|All|N/A|
|VR.911|Charge description is mandatory|E0H|All|N/A|
|VR.912|Resolution is mandatory|E0H|All|N/A|
|VR.915|Transparent invoicing is mandatory|E0H|All|N/A|
|VR.916|Tax indicator is mandatory|E0H|All|N/A|
|VR.917|Termination date and effective date must have the same value|E0H|All|N/A|
|VR.919|The time interval (start and end) of the price series must equal midnight local time, expressed in UTC+0.|E86|All|N/A|
|VR.920|Tax indicator must be set to false for a subscription|D14|All|N/A|
|VR.921|Tax indicator must be set to false for a fee|D14|All|N/A|
|VR.922|The identification of a charge operation consists of maximal 36 characters|E86|All|N/A|

* VR.152 is not fully implemented. Right now we only validate that it is filled with something
* VR.679 is not fully implemented. For now it verifies that the charge exist, not checking that the linked period is within the charge's validity period
* VR.902-x are temporary and will be changed once update and stop functionality has been implemented.
* N/A Means the validation rule is not applicable to that subdomain
