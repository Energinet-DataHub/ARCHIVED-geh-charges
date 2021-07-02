# Protocol Documentation

## Table of Contents

- [IntegrationEventContract.proto](#IntegrationEventContract.proto)
    - [ChargeCreated](#.ChargeCreated)
    - [ChargePeriodsUpdated](#.ChargePeriodsUpdated)
    - [ChargeDiscontinued](#.ChargeDiscontinued)
    - [ChargeDiscontinuationCancelled](#.ChargeDiscontinuationCancelled)
    - [ChargePricesUpdated](#.ChargePricesUpdated)

<a name="IntegrationEventContract.proto"></a>

## IntegrationEventContract.proto

Charges Domain related integration events

<a name=".ChargeCreated"></a>

### ChargeCreated

Represents the creation of a new charge.
This event contains the static charge data as well initial charge period data.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string |  | A charge identifier provided by the Market Participant. Combined with Charge Owner it becomes unique  |
| ChargeType | enum |  | The type of charge; tariff, fee or subscription |
| ChargeOwner | string |  | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| Currency | string |  | The charge price currency |
| Resolution | enum |  | The resolution of a charge price list, e.g. 15 mins, hourly, daily, monthly |
| TaxIndicator | bit |  | Indicates whether a tariff is considered a tax or not, 1 = true |
| StartDateTime | Timestamp (UTC) |  | The charge period's valid from date and time |
| EndDateTime | Timestamp (UTC), nullable |  | The charge period's valid to date and time |
| CorrelationId | string |  | The associated correlation id of the processed charge request |
|  |  |  |  |

<a name=".ChargePeriodsUpdated"></a>

### ChargePeriodsUpdated

Represents the update of charge periods details.
This event contains the new period as well other periods that were adjusted by it.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string |  | A charge identifier provided by the Market Participant. Combined with Charge Owner it becomes unique  |
| ChargeType | enum |  | The type of charge; tariff, fee or subscription |
| ChargeOwner | string |  | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| CompleteUpdatePeriodStartDateTime | Timestamp (UTC) |  | The starting point of the earliest charge period reported in this event |
| CompleteUpdatePeriodEndDateTime | Timestamp (UTC) |  | The end point of the latest charge period reported in this event, if that period's EndDateTime is null, 31/12/9999 is used |
| CorrelationId | string |  | The associated correlation id of the processed charge request, it is the same for all charge periods below |
| ChargePeriods |  |  | A list with the updated charge period as well as any adjusted charge periods |
| StartDateTime | Timestamp (UTC) |  | The charge period's valid from date and time |
| EndDateTime | Timestamp (UTC), nullable |  | The charge period's valid to date and time |
|  |  |  |  |

<a name=".ChargeDiscontinued"></a>

### ChargeDiscontinued

Represents the discontinuation of a charge.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string |  | A charge identifier provided by the Market Participant. Combined with Charge Owner it becomes unique  |
| ChargeType | enum |  | The type of charge; tariff, fee or subscription |
| ChargeOwner | string |  | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| CorrelationId | string |  | The associated correlation id of the processed charge request |
| Discontinued (TBD) | bit |  | Undecided |

<a name=".ChargeDiscontinuationCancelled"></a>

### ChargeDiscontinuationCancelled

Represents the cancellation of the charge discontinuation.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
|  | string |  |  |
| ChargeId | string |  | A charge identifier provided by the Market Participant. Combined with Charge Owner it becomes unique  |
| ChargeType | enum |  | The type of charge; tariff, fee or subscription |
| ChargeOwner | string |  | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| CorrelationId | string |  | The associated correlation id of the processed charge request |
| Discontinued (TBD) | bit |  | Undecided |

<a name=".ChargePricesUpdated"></a>

### ChargePricesUpdated

Represents the update of one or more charge prices.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string |  | A charge identifier provided by the Market Participant. Combined with Charge Owner it becomes unique  |
| ChargeType | enum |  | The type of charge; tariff, fee or subscription |
| ChargeOwner | string |  | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| PriceListStartDateTime | Timestamp (UTC) |  |  |
| PriceListEndDateTime | Timestamp (UTC) |  |  |
| Time | Timestamp (UTC) |  | A point in time where the charge price applies |
