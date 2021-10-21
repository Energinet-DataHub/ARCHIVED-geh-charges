# Protocol Documentation

## Table of Contents

- [IntegrationEventContract.proto](#IntegrationEventContract.proto)
- [Charges and charge prices events](#.ChargesAndChargePricesEvents)
    - [ChargeCreated](#.ChargeCreated)
    - [ChargePeriodUpdated](#.ChargePeriodUpdated)
        - [ChargePeriod](#.ChargePeriod)
    - [ChargeDiscontinued](#.ChargeDiscontinued)
    - [ChargeDiscontinuationCancelled](#.ChargeDiscontinuationCancelled)
    - [ChargePricesUpdated](#.ChargePricesUpdated)
        - [ChargePrice](#.ChargePrice)
- [Charge link events](#.ChargeLinkEvents)
    - [ChargeLinkCreated](#.ChargeLinkCreated)
        - [ChargeLinkPeriod](#.ChargeLinkPeriod)
    - [ChargeLinkUpdated](#.ChargeLinkUpdated)
- [Charge link requests and replies](#.ChargeLinkRequestsAndReplies)
    - [CreateDefaultChargeLinks](#.CreateDefaultChargeLinks)
    - [CreateDefaultChargeLinksSucceeded](#.CreateDefaultChargeLinksSucceeded)
    - [CreateDefaultChargeLinksFailed](#.CreateDefaultChargeLinksFailed)
    - [CreateDefaultChargeLinkMessages](#.CreateDefaultChargeLinkMessages)
    - [CreateDefaultChargeLinkMessagesSucceeded](#.CreateDefaultChargeLinkMessagesSucceeded)
    - [CreateDefaultChargeLinkMessagesFailed](#.CreateDefaultChargeLinkMessagesFailed)

<a name="IntegrationEventContract.proto"></a>

## IntegrationEventContract.proto

Charges domain related integration events.

Note: The integration events adhere to the architecture decision record ([ADR-0008](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/architecture-decision-record/ADR-0008%20Integration%20events.md)), which among other things defines the integration event's [meta data](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/docs/architecture-decision-record/ADR-0008%20Integration%20events.md#message-metadata).

<a name=".ChargesAndChargePricesEvents"></a>

## Charges and charge prices events

<a name=".ChargeCreated"></a>

### [ChargeCreated](.././source/GreenEnergyHub.Charges/source/GreenEnergyHub.Charges.Infrastructure/Integration/Charge/ChargeCreated.proto)

Represents the creation of a new charge.

<a name=".ChargePeriodUpdated"></a>

### ChargePeriodUpdated

Represents the update of a charge period.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| ChargeId | string | required | A charge identifier provided by the Market Participant. Combined with Charge Owner and Charge Type it becomes unique  |
| ChargeType | enum | required | The type of charge; tariff, fee or subscription |
| ChargeOwner | string | required | A charge owner identification, e.g. the Market Participant's GLN or EIC number |
| ChargePeriod | [ChargePeriod](#.ChargePeriod) | required | Contains the charge's changed validity period |

<a name=".ChargePeriod"></a>

#### ChargePeriod

Represents a charge period.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
| StartDateTime | Timestamp | required | In UTC. The charge period's valid from date and time |
| EndDateTime | Timestamp | optional with default value | In UTC. The charge period's valid to date and time. The default value will be the equivalent to 9999-12-31T23:59:59Z without milliseconds |

The Timestamp type is documented [here](https://developers.google.com/protocol-buffers/docs/reference/google.protobuf#timestamp).

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

### [ChargePricesUpdated](.././source/GreenEnergyHub.Charges/source/GreenEnergyHub.Charges.Infrastructure/Integration/Charge/ChargePricesUpdated.proto)

Represents the creation and update of one or more charge prices.

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

<a name=".CreateDefaultChargeLinks"></a>

### [CreateDefaultChargeLinks](.././source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients/DefaultChargeLink/Contracts/CreateDefaultChargeLinks.proto)

This request is used by the Metering Point domain as part of the 'Create metering point' process to request the Charges domain to link default charges a the newly created metering point.

<a name=".CreateDefaultChargeLinksSucceeded"></a>

### [CreateDefaultChargeLinksSucceeded](.././source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients/DefaultChargeLink/Contracts/CreateDefaultChargeLinksSucceeded.proto)

The Charges domain will use this reply to inform the Metering Point domain when the `CreateDefaultChargeLinks` request has been processed successfully.

<a name=".CreateDefaultChargeLinksFailed"></a>

### [CreateDefaultChargeLinksFailed](.././source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients/DefaultChargeLink/Contracts/CreateDefaultChargeLinksFailed.proto)

In case the Charges domain fails to process a `CreateDefaultChargeLinks` request this reply will be used to inform the Metering Point domain.

<a name=".CreateDefaultChargeLinkMessages"></a>

### [CreateDefaultChargeLinkMessages](.././source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients/DefaultChargeLinkMessages/Contracts/CreateDefaultChargeLinkMessages.proto)

This request is used by the Metering Point domain as part of the 'Create metering point' process to request the Charges domain to create messages containing the default charge links for a specific metering point and make them available to the relevant Market Participants.

<a name=".CreateDefaultChargeLinkMessagesSucceeded"></a>

### [CreateDefaultChargeLinkMessagesSucceeded](.././source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients/DefaultChargeLinkMessages/Contracts/CreateDefaultChargeLinkMessagesSucceeded.proto)

The Charges domain will use this reply to inform the Metering Point domain when the `CreateDefaultChargeLinkMessages` request has been processed successfully.

<a name=".CreateDefaultChargeLinkMessagesFailed"></a>

### [CreateDefaultChargeLinkMessagesFailed](.././source/Energinet.Charges.Libraries/source/Energinet.DataHub.Charges.Clients/DefaultChargeLinkMessages/Contracts/CreateDefaultChargeLinkMessagesFailed.proto)

In case the Charges domain fails to process a `CreateDefaultChargeLinkMessages` request this reply will be used to inform the Metering Point domain.
