# Mapping RSM-034 from ebIX to CIM

## Mapping table

The mapping covers the following RSM-034 messages and adheres to the general [rsm-to-cim.md](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/samples/energinet/docs/document-type-mappings/rsm-to-cim.md) document.

>- **`DK_NotifyChargeInformation`** - Notification of price list changes

Fields shared by all RSM messages are omitted from this document. They can be found [here](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/samples/energinet/docs/document-type-mappings/shared-rsm-fields.md)

> Do note, that some of the CIM names are identical and will require a context to acquire uniqueness, hence a suggested context is provided for those mappings.

### `DK_NotifyChargeInformation`

| **EbIX attribute**|**CIM name**| **Suggested "context" if needed to obtain CIM name uniqueness** | **CIM path** |
|:-|:-|:-|:-|
| PayloadChargeEvent/Identification | mRID | | MktActivityRecord/mRID |
| Occurrence | `dateTime` | ValidityStart_| `MktActivityRecord/ValidityStart_DateAndOrTime/dateTime` |
| Function | status | MktActivityRecord_ | MktActivityRecord/status |
| ChargeType | type |  | Series/ChargeType/type|
| PartyChargeTypeID | mRID | ChargeType_ | Series/ChargeType/mRID |
| Description | name | | MktActivityRecord/ChargeType/name |
| LongDescription | description | | MktActivityRecord/ChargeType/description |
| VATClass | VATPayer | | MktActivityRecord/ChargeType/VATPayer |
| TransparentInvoicing | TransparentInvoicing | | MktActivityRecord/ChargeType/TransparentInvoicing |
| TaxIndicator | TaxIndicator | | MktActivityRecord/ChargeType/TaxIndicator |
| Position | Position | | TimeSeries/Period/Point/Position |
| EnergyPrice | amount | | TimeSeries/Period/Point/Price/amount |
| ResolutionDuration | resolution | | TimeSeries/Period/Resolution |
| ChargeTypeOwnerEnergyParty/Identification | mRID | ChargeTypeOwner_| Series/ChargeType/ChargeTypeOwner_MarketParticipant/mRID |
