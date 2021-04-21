# Shared RSM Mapping from ebIX to CIM

## Introduction

In Denmark the Market Actors communicate with DataHub using a set of RSM-messages (SOAP) that follow the industrial standard of [ebIX](https://www.ebix.org/). Once received by DataHub, a message will be converted to an internal format, a Data Transfer Object, which conforms to the naming convention of the CIM standard. When a message is sent from DataHub to a Market Actor this will yet again be following the ebIX standard.

## Motivation

The purpose of this document is create a single mapping reference for the shared fields between all RSM documents.

## Mapping table

The mapping covers the generic set of attributes shared amongst all the RSM-messages communicated to and from DataHub. It adheres to the general [rsm-to-cim.md](https://github.com/Energinet-DataHub/green-energy-hub/blob/main/samples/energinet/docs/document-type-mappings/rsm-to-cim.md) document.

> Do note, that some of the CIM names are identical and will require a context to acquire uniqueness, hence a suggested context is provided for those mappings.

| **EbIX attribute**|**CIM name**| **Suggested "context" if needed to obtain CIM name uniqueness** | **CIM path** |
|:-|:-|:-|:-|
| MessageReference | No CIM name, use ebIX |||
| DocumentType | No CIM name, use ebIX |||
| MessageType | No CIM name, use ebIX |||
| Identification | mRID | MarketDocument | MarketDocument/mRID |
| DocumentType | Type | MarketDocument | MarketDocument/Type |
| Creation | CreatedDateTime | MarketDocument | MarketDocument/CreatedDateTime |
| Sender/Identification | mRID | SenderMarketParticipant | MarketDocument/Sender_MarketParticipant/mRID |
| Recipient/Identification | mRID | ReceiverMarketParticipant | MarketDocument/Receiver_MarketParticipant/mRID |
| EnergyBusinessProcess | ProcessType | MarketDocument | MarketDocument/Process/ProcessType |
| EnergyBusinessProcessRole (Sender) | Type | SenderMarketParticipant<br>or SenderMarketParticipant_(MarketRole) | MarketDocument/Sender_MarketParticipant/MarketRole/Type |
| EnergyBusinessProcessRole (Recipient) | Type | ReceiverMarketParticipant<br>or ReceiverMarketParticipant_(MarketRole) | MarketDocument/Receiver_MarketParticipant/MarketRole/Type |
| EnergyIndustryClassification | Kind | MarketServiceCategory | MarketDocument/Market_ServiceCategory/Kind |
