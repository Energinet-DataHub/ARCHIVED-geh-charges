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
|  | string |  | Unique metering point identifier of the metering point which has its settlement details changed. |
|  |  |  |  |
|  |  |  |  |

<a name=".ChargePeriodsUpdated"></a>

### ChargePeriodsUpdated

Represents the update of charge periods details.
This event contains the new period as well other periods that were adjusted by it.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
|  | string |  | Unique metering point identifier of the metering point which has its settlement details changed. |
|  |  |  |  |
|  |  |  |  |

<a name=".ChargeDiscontinued"></a>

### ChargeDiscontinued

Represents the discontinuation of a charge.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
|  | string |  | Unique metering point identifier of the metering point which has its settlement details changed. |
|  |  |  |  |
|  |  |  |  |

<a name=".ChargeDiscontinuationCancelled"></a>

### ChargeDiscontinuationCancelled

Represents the cancellation of the charge discontinuation.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
|  | string |  | Unique metering point identifier of the metering point which has its settlement details changed. |
|  |  |  |  |
|  |  |  |  |

<a name=".ChargePricesUpdated"></a>

### ChargePricesUpdated

Represents the update of one or more charge prices.

| Field | Type | Label | Description |
| ----- | ---- | ----- | ----------- |
|  | string |  | Unique metering point identifier of the metering point which has its settlement details changed. |
|  |  |  |  |
|  |  |  |  |
