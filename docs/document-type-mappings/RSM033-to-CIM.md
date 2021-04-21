# Mapping RSM-033 from ebIX to CIM

## Mapping table

The mapping covers the following RSM-033 messages and adheres to the general [rsm-to-cim.md](https://github.com/Energinet-DataHub/geh-charges/blob/main/docs/document-type-mappings/rsm-to-cim.md) document.

>- **DK_RequestUpdateChargeInformation**
>- **DK_ConfirmUpdateChargeInformation**
>- **DK_RejectUpdateChargeInformation**

Fields shared by all RSM messages are omitted from this document. They can be found [here](https://github.com/Energinet-DataHub/geh-charges/blob/main/docs/document-type-mappings/shared-rsm-fields.md)

> Do note, that some of the CIM names are identical and will require a context to acquire uniqueness, hence a suggested context is provided for those mappings.

### DK_RequestUpdateChargeInformation

| **EbIX attribute**|**CIM name**| **Suggested "context" if needed to obtain CIM name uniqueness** | **CIM path** |
|:-|:-|:-|:-|
| PayloadChargeEvent/Identification | mRID | | MktActivityRecord/mRID |
| Occurrence | DateTime | ValidityStart_| MktActivityRecord/ValidityStart_DateAndOrTime/DateTime |
| Function | Status | MktActivityRecord_ | MktActivityRecord/status |
| ChargeType | Type |  | Series/ChargeType/type|
| PartyChargeTypeID | mRID | ChargeType_ | Series/ChargeType/mRID |
| Description | Name | | MktActivityRecord/ChargeType/name |
| LongDescription | Description | | MktActivityRecord/ChargeType/description |
| VATClass | VATPayer | | MktActivityRecord/ChargeType/VATPayer |
| TransparentInvoicing | TransparentInvoicing | | MktActivityRecord/ChargeType/TransparentInvoicing |
| TaxIndicator | TaxIndicator | | MktActivityRecord/ChargeType/TaxIndicator |
| Position | Position | | Period/Point/Position |
| EnergyPrice | Amount | | Period/Point/Price/Amount |
| ResolutionDuration | Resolution | | Period/Resolution |
| ChargeTypeOwnerEnergyParty/Identification | mRID | ChargeTypeOwner_| Series/ChargeType/ChargeTypeOwner_MarketParticipant/mRID |

### DK_ConfirmUpdateChargeInformation

| **EbIX attribute**|**CIM name**| **Suggested "context" if needed to obtain CIM name uniqueness** | **CIM path** |
|:-|:-|:-|:-|
| Identification | mRID | | MktActivityRecord/mRID |
| OriginalBusinessDocument | mRID | OriginalTransactionReference | MktActivityRecord/OriginalTransactionReference_MktActivityRecord/mRID |
| StatusType | Code || Reason/Code |

### DK_RejectUpdateChargeInformation

| **EbIX attribute**|**CIM name**| **Suggested "context" if needed to obtain CIM name uniqueness** | **CIM path** |
|:-|:-|:-|:-|
| Identification | mRID | |MktActivityRecord/mRID |
| OriginalBusinessDocument | mRID | OriginalTransactionReference | MktActivityRecord/OriginalTransactionReference_MktActivityRecord/mRID |
| StatusType | Code || Reason/Code |
| ResponseReasonType | Reason || MktActivityRecord/Reason|
