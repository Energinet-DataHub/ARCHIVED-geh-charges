BEGIN TRANSACTION

DELETE T
FROM
(
SELECT *
, DupRank = ROW_NUMBER() OVER (
              PARTITION BY ChargeId, MeteringPointId, StartDateTime
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
              PARTITION BY ChargeId, MeteringPointId, EndDateTime
              ORDER BY (SELECT NULL)
            )
FROM [Charges].[ChargeLink]
) AS T
WHERE DupRank > 1 

GO

ALTER TABLE [Charges].[ChargeLink]
    ADD CONSTRAINT UQ_DefaultOverlap_StartDateTime UNIQUE (ChargeId, MeteringPointId, StartDateTime);
GO

ALTER TABLE [Charges].[ChargeLink]
    ADD CONSTRAINT UQ_DefaultOverlap_EndDateTime UNIQUE (ChargeId, MeteringPointId, EndDateTime);
GO

COMMIT