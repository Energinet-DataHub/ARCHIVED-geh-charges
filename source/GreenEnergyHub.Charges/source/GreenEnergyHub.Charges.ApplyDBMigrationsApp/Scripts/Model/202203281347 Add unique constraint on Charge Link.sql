BEGIN TRANSACTION

    IF OBJECT_ID('[Charges].[UQ_DefaultOverlap_StartDateTime]', 'UQ') IS NULL
        Begin
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
        end
    GO
    
    IF OBJECT_ID('[Charges].[UQ_DefaultOverlap_StartDateTime]', 'UQ') IS NULL
    Begin
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
    End
    
    GO
    
    IF OBJECT_ID('[Charges].[UQ_DefaultOverlap_StartDateTime]', 'UQ') IS NULL
    Begin
    
        ALTER TABLE [Charges].[ChargeLink]
            ADD CONSTRAINT UQ_DefaultOverlap_StartDateTime UNIQUE (ChargeId, MeteringPointId, StartDateTime)
    End
    GO
    
    IF OBJECT_ID('[Charges].[UQ_DefaultOverlap_EndDateTime]', 'UQ') IS NULL
        Begin
            ALTER TABLE [Charges].[ChargeLink]
                ADD CONSTRAINT UQ_DefaultOverlap_EndDateTime UNIQUE (ChargeId, MeteringPointId, EndDateTime)
        End
    GO

COMMIT