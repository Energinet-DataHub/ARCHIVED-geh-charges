BEGIN TRANSACTION

ALTER TABLE
    DROP CONSTRAINT UQ_DefaultOverlap_StartDateTime

ALTER TABLE
    DROP CONSTRAINT UQ_DefaultOverlap_EndDateTime

DELETE T
FROM
(
SELECT *
, DupRank = ROW_NUMBER() OVER (
              PARTITION BY ChargeInformationId, MeteringPointId, StartDateTime
              ORDER BY (SELECT NULL)
            )
FROM [Charges].[ChargeLink]
) AS T
WHERE DupRank > 1

GO

DELETE T
FROM
(
SELECT *
, DupRank = ROW_NUMBER() OVER (
              PARTITION BY ChargeInformationId, MeteringPointId, EndDateTime
              ORDER BY (SELECT NULL)
            )
FROM [Charges].[ChargeLink]
) AS T
WHERE DupRank > 1 

GO

ALTER TABLE [Charges].[ChargeLink]
    ADD CONSTRAINT UQ_DefaultOverlap_StartDateTime UNIQUE (ChargeInformationId, MeteringPointId, StartDateTime);
GO

ALTER TABLE [Charges].[ChargeLink]
    ADD CONSTRAINT UQ_DefaultOverlap_EndDateTime UNIQUE (ChargeInformationId, MeteringPointId, EndDateTime);
GO

COMMIT