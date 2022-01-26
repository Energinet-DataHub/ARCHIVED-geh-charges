ALTER TABLE Charges.MeteringPoint
    ALTER COLUMN GridAreaId uniqueidentifier

ALTER TABLE Charges.MeteringPoint
    ADD FOREIGN KEY (GridAreaId) REFERENCES Charges.GridArea(Id);

-- TODO BJARKE: Fix bad meteringpoint.gridareaid in u and t
