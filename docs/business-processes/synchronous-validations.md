# Charges synchronous validations

Just checking CI. Will close PR again.

The following synchronous validation rules are currently implemented in the charge domain.

**Description**|**Status Code**|**Charge Types**|**Charge Links**|
|:-|:-|:-|:-|
|Request must adhere to CIM XML schema|400 Bad Request|All|All|
|Authenticated user must match provided sender organisation|400 Bad Request|All|N/A|

* N/A Means the validation rule is not applicable to that subdomain
