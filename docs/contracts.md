# Protocol Documentation

## Table of Contents

- [IntegrationEventContract.proto](#IntegrationEventContract.proto)
- [Charges and charge prices events](#.ChargesAndChargePricesEvents)
    - [ChargeCreated](#.ChargeCreated)
        - [ChargePeriod](#.ChargePeriod)
    - [ChargePeriodUpdated](#.ChargePeriodUpdated)
    - [ChargeDiscontinued](#.ChargeDiscontinued)
    - [ChargeDiscontinuationCancelled](#.ChargeDiscontinuationCancelled)
    - [ChargePricesUpdated](#.ChargePricesUpdated)
        - [ChargePrice](#.ChargePrice)
- [Charge link events](#.ChargeLinkEvents)
    - [ChargeLinkCreated](#.ChargeLinkCreated)
        - [ChargeLinkPeriod](#.ChargeLinkPeriod)
    - [ChargeLinkUpdated](#.ChargeLinkUpdated)
- [Charge link requests and replies](#.ChargeLinkRequestsAndReplies)
    - [CreateDefaultChargeLinksRequest](#.CreateDefaultChargeLinksRequest)
    - [CreateDefaultChargeLinksSucceeded](#.CreateDefaultChargeLinksSucceeded)
    - [CreateDefaultChargeLinksFailed](#.CreateDefaultChargeLinksFailed)
    - [CreateDefaultChargeLinkMessagesRequest](#.CreateDefaultChargeLinkMessagesRequest)
    - [CreateDefaultChargeLinkMessagesSucceeded](#.CreateDefaultChargeLinkMessagesSucceeded)
    - [CreateDefaultChargeLinkMessagesFailed](#.CreateDefaultChargeLinkMessagesFailed)

<a name="IntegrationEventContract.proto"></a>

## IntegrationEventContract.proto

Charges Domain related integration events

Note: Correlation Id is expected to be available on integration events as part of their meta data and for that reason it is not reflected below.

<br>
<a name=".ChargesAndChargePricesEvents"></a>

## Charges and charge prices events

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
| TaxIndicator | bool | required | Indicates whether a tariff is considered a tax or not |
| ChargePeriod | [ChargePeriod](#.ChargePeriod) | required | Contains the charge's validity period  |

<a name=".ChargePeriod"></a>

#### ChargePeriod

Represents a charge period.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| StartDateTime | Timestamp | required | In UTC. The charge period's valid from date and time |
| EndDateTime | Timestamp | optional with default value | In UTC. The charge period's valid to date and time. The default value will be the equivalent to 9999-12-31T23:59:59Z without milliseconds |

The Timestamp type is documented [here](https://developers.google.com/protocol-buffers/docs/reference/google.protobuf#timestamp).

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
| EndDateTime | Timestamp | required | In UTC. The date and time the charge is discontinued |

<a name=".ChargeDiscontinuationCancelled"></a>

### ChargeDiscontinuationCancelled

Represents the cancellation of the charge discontinuation.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string | required | A charge identifier provided by the Market Participant. Combined with Charge Owner and Charge Type it becomes unique  |
| ChargeType | enum | required | The type of charge; tariff, fee or subscription |
| ChargeOwner | string | required | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| CancelledEndDateTime | Timestamp | required | In UTC. The date and time that the cancellation applies (expected to hit a previous discontinuation) |

<a name=".ChargePricesUpdated"></a>

### ChargePricesUpdated

Represents the creation and update of one or more charge prices.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string | required | A charge identifier provided by the Market Participant. Combined with Charge Owner and Charge Type it becomes unique  |
| ChargeType | enum | required | The type of charge; tariff, fee or subscription |
| ChargeOwner | string | required | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| UpdatedPeriodStartDateTime | Timestamp | required | In UTC. The start of the charge prices period. The start equals the time of the earliest charge price in the list |
| UpdatedPeriodEndDateTime | Timestamp | required | In UTC. The end of the charge prices period. The end is to be considered an up to (excluding) date time which equals the end of the latest charge price in the list. This is a calculated value that adds a single duration equal to the charge's resolution, e.g. hourly, to the latest charge price's time |
| Points | [ChargePrice](#.ChargePrice) | required | A list with charge prices |

<a name=".ChargePrice"></a>

#### ChargePrice

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| Time | Timestamp | required | In UTC. Point in time where the charge price applies |
| Price | Decimal(14,6) | required | Charge price |

<br>
<a name=".ChargeLinkEvents"></a>

## Charge link events

<a name=".ChargeLinkCreated"></a>

### ChargeLinkCreated

Represents the creation of a new charge link

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeLinkId | string | required | An identifier for the charge link event. Provided by the Market Participant. Uniqueness cannot be guaranteed |
| MeteringPointId | string | required | A unique metering point identifier |
| ChargeId | string | required | A charge identifier. Combined with Charge Owner and Charge Type it becomes unique  |
| ChargeType | enum | required | The type of charge; tariff, fee or subscription |
| ChargeOwner | string | required | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| ChargeLinkPeriod | [ChargeLinkPeriod](#.ChargeLinkPeriod) | required | Contains the charge link's validity period and factor (quantity) |

<a name=".ChargeLinkPeriod"></a>

#### ChargeLinkPeriod

Represents a charge link period.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| StartDateTime | Timestamp | required | In UTC. The charge link period's valid from date and time |
| EndDateTime | Timestamp | optional with default value | In UTC. The charge link period's valid to date and time. The default value will be the equivalent to 9999-12-31T23:59:59Z without milliseconds |
| Factor | int | required | Indicates the number of times the same fee or subscription must be collected. Always 1 for tariffs |

<a name=".ChargeLinkUpdated"></a>

### ChargeLinkUpdated

Represents the update of one or more charge links.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeLinkId | string | required | An identifier for the charge link event. Provided by the Market Participant. Uniqueness cannot be guaranteed |
| MeteringPointId | string | required | A unique metering point identifier |
| ChargeId | string | required | A charge identifier. Combined with Charge Owner and Charge Type it becomes unique  |
| ChargeType | enum | required | The type of charge; tariff, fee or subscription |
| ChargeOwner | string | required | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| UpdatedPeriodStartDateTime | Timestamp | required | In UTC. Time interval covering the entire period of charge link updates within this event. The start equals the StartDateTime of the earliest charge link in the Periods list |
| UpdatedPeriodEndDateTime | Timestamp | required | In UTC. Time interval covering the entire period of charge link updates within this event. The end equals the EndDateTime of the latest charge link in the Periods list |
| Periods | [ChargeLinkPeriod](#.ChargeLinkPeriod) | required | A list of charge link periods and factor values |

<br>
<a name=".ChargeLinkRequestsAndReplies"></a>

## Charge link requests and replies

<a name=".CreateDefaultChargeLinksRequest"></a>

### CreateDefaultChargeLinksRequest

Used by Metering Point domain as part of the 'Create metering point' process to request the Charges domain to link default charges to the newly created metering point.
Go to the [contract](.././source/GreenEnergyHub.Charges/source/GreenEnergyHub.Charges.Infrastructure/Integration/ChargeLink/Commands/CreateDefaultChargeLinks.proto).

<a name=".CreateDefaultChargeLinksSucceeded"></a>

### CreateDefaultChargeLinksSucceeded

Go to [contract](.././source/GreenEnergyHub.Charges/source/GreenEnergyHub.Charges.Infrastructure/Integration/ChargeLink/Commands/CreateDefaultChargeLinksSucceeded.proto).

<a name=".CreateDefaultChargeLinksFailed"></a>

### CreateDefaultChargeLinksFailed

Go to [contract](.././source/GreenEnergyHub.Charges/source/GreenEnergyHub.Charges.Infrastructure/Integration/ChargeLink/Commands/CreateDefaultChargeLinksFailed.proto).

<a name=".CreateDefaultChargeLinkMessagesRequest"></a>

### CreateDefaultChargeLinkMessagesRequest

Go to [contract](.././source/GreenEnergyHub.Charges/source/GreenEnergyHub.Charges.Infrastructure/Integration/ChargeLink/Commands/CreateDefaultChargeLinkMessages.proto).

<a name=".CreateDefaultChargeLinkMessagesSucceeded"></a>

### CreateDefaultChargeLinkMessagesSucceeded

Go to [contract](.././source/GreenEnergyHub.Charges/source/GreenEnergyHub.Charges.Infrastructure/Integration/ChargeLink/Commands/CreateDefaultChargeLinkMessagesSucceeded.proto).

<a name=".CreateDefaultChargeLinkMessagesFailed"></a>

### CreateDefaultChargeLinkMessagesFailed

Go to [contract](.././source/GreenEnergyHub.Charges/source/GreenEnergyHub.Charges.Infrastructure/Integration/ChargeLink/Commands/CreateDefaultChargeLinkMessagesFailed.proto).
