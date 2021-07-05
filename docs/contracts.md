# Protocol Documentation

## Table of Contents

- [IntegrationEventContract.proto](#IntegrationEventContract.proto)
    - [ChargeCreated](#.ChargeCreated)
        - [ChargePeriod](#.ChargePeriod)
    - [ChargePeriodUpdated](#.ChargePeriodUpdated)
    - [ChargeDiscontinued](#.ChargeDiscontinued)
    - [ChargeDiscontinuationCancelled](#.ChargeDiscontinuationCancelled)
    - [ChargePricesUpdated](#.ChargePricesUpdated)
        - [ChargePrice](#.ChargePrice)

<a name="IntegrationEventContract.proto"></a>

## IntegrationEventContract.proto

Charges Domain related integration events

Note: Correlation Id is expected to be available on integration events as part of their meta data and for that reason it is not reflected below.

<a name=".ChargeCreated"></a>

### ChargeCreated

Represents the creation of a new charge.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string | required | A charge identifier provided by the Market Participant. Combined with Charge Owner and Charge Type it becomes unique  |
| ChargeType | enum | required | The type of charge; tariff, fee or subscription |
| ChargeOwner | string | required | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| Currency | string | required | The charge price currency |
| Resolution | enum | required | The resolution of a charge price list, e.g. 15 min, hourly, daily, monthly |
| TaxIndicator | bit | required | Indicates whether a tariff is considered a tax or not, 1 = true |
| ChargePeriod | [ChargePeriod](#.ChargePeriod) | required | Contains the charge's validity period  |

<a name=".ChargePeriod"></a>

#### ChargePeriod

Represents a charge period.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| StartDateTime | Timestamp (UTC) | required | The charge period's valid from date and time |
| EndDateTime | Timestamp (UTC), nullable | optional | The charge period's valid to date and time |

<a name=".ChargePeriodUpdated"></a>

### ChargePeriodUpdated

Represents the update of a charge period.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string | required | A charge identifier provided by the Market Participant. Combined with Charge Owner and Charge Type it becomes unique  |
| ChargeType | enum | required | The type of charge; tariff, fee or subscription |
| ChargeOwner | string | required | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| ChargePeriod | [ChargePeriod](#.ChargePeriod) | required | Contains the charge's changed validity period |

<a name=".ChargeDiscontinued"></a>

### ChargeDiscontinued

Represents the discontinuation of a charge.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string | required | A charge identifier provided by the Market Participant. Combined with Charge Owner and Charge Type it becomes unique  |
| ChargeType | enum | required | The type of charge; tariff, fee or subscription |
| ChargeOwner | string | required | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| EndDateTime | Timestamp (UTC) | required | The date and time the charge is discontinued |

<a name=".ChargeDiscontinuationCancelled"></a>

### ChargeDiscontinuationCancelled

Represents the cancellation of the charge discontinuation.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string | required | A charge identifier provided by the Market Participant. Combined with Charge Owner and Charge Type it becomes unique  |
| ChargeType | enum | required | The type of charge; tariff, fee or subscription |
| ChargeOwner | string | required | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| CancelledEndDateTime | Timestamp (UTC) | required | The date and time that the cancellation applies (expected to hit a previous discontinuation) |

<a name=".ChargePricesUpdated"></a>

### ChargePricesUpdated

Represents the update of one or more charge prices.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string | required | A charge identifier provided by the Market Participant. Combined with Charge Owner and Charge Type it becomes unique  |
| ChargeType | enum | required | The type of charge; tariff, fee or subscription |
| ChargeOwner | string | required | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| UpdatedPeriodStartDateTime | Timestamp (UTC) | required | The time interval covers the entire period of charge prices, the starting point here is equal to the time of the very first charge price in the list  |
| UpdatedPeriodEndDateTime | Timestamp (UTC) | required | The time interval covers the entire period of charge prices, the end point here is equal to the time of the last charge price in the list |
| Points | [ChargePrice](#.ChargePrice) | required | A list with charge prices |

<a name=".ChargePrice"></a>

#### ChargePrice

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Time | Timestamp (UTC) | required | Point in time where the charge price applies |
| Price | Decimal(14,6) | required | Charge price |
