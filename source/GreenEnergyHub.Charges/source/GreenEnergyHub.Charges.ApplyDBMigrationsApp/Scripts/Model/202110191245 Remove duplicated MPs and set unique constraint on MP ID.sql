/* 
This script deletes any duplicate metering point records in the MeteringPoint table (based on MeteringPointId)
It ensures that the one metering point that has Charge Links is kept.
Note: It has been verified that it is only possible to link a Charge to a metering point while there is no metering point duplicates in the table.
If none of the records have Charge Links it keeps one record (which one depends on the query result)
*/
DELETE
FROM Charges.MeteringPoint
WHERE ID IN (
	SELECT Id
	FROM (
		SELECT 
			P.MeteringPointId,
			MP.Id,
			CL.Id AS LinkId,
			L.Links,
			ROW_NUMBER() OVER (PARTITION BY P.MeteringPointId ORDER BY MP.Id ASC) AS RowNumber
		FROM (
			SELECT MeteringPointId, COUNT(*) AS NoWithId -- Find out how many times each metering point ID exist in the DB
			FROM Charges.MeteringPoint
			GROUP BY MeteringPointId
		) P
		LEFT OUTER JOIN Charges.MeteringPoint MP ON MP.MeteringPointId = P.MeteringPointId
		LEFT OUTER JOIN Charges.ChargeLink CL ON MP.Id = CL.MeteringPointId
		LEFT OUTER JOIN (
			SELECT
			CMP.MeteringPointId,
			COUNT(*) AS Links
			FROM Charges.ChargeLink
			LEFT OUTER JOIN Charges.MeteringPoint CMP ON CMP.Id = Charges.ChargeLink.MeteringPointId
			GROUP BY CMP.MeteringPointId
		) L ON L.MeteringPointId = P.MeteringPointId
		WHERE
			P.NoWithId > 1 -- Only work with metering point IDs that are represented more than once in the database
			AND CL.Id IS NULL -- Only work with metering point IDs that does not have links
	) D
	WHERE
		((D.Links IS NULL) AND RowNumber > 1)
		OR ((D.Links IS NOT NULL) AND (RowNumber > 0))
)
GO

-- Set Unique constraint on MeteringPointId
ALTER TABLE [Charges].[MeteringPoint]
ADD CONSTRAINT UC_MeteringPointId UNIQUE (MeteringPointId);
GO