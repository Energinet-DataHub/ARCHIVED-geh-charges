CREATE TABLE ChargeType
(
    ID int NOT NULL PRIMARY KEY,
    Code nvarchar(12) NOT NULL CHECK (Code IN('D01', 'D02', 'D03')),
    Name nvarchar(12) NOT NULL,
)
GO

CREATE TABLE ResolutionType
(
    ID int NOT NULL PRIMARY KEY,
    name nvarchar(5) NOT NULL CHECK (name IN('PT15M', 'PT1H', 'P1D', 'P1M'))
)
GO

CREATE TABLE VATPayerType
(
    ID int NOT NULL PRIMARY KEY,
    name nvarchar(3) NOT NULL CHECK (name IN('D01', 'D02'))
)
GO

CREATE TABLE MarketParticipant
(
    ID int NOT NULL IDENTITY PRIMARY KEY,
    mRID nvarchar(35) NOT NULL,
    Name nvarchar(100) NOT NULL,
    Role nvarchar(3) NOT NULL CHECK (Role IN('DDQ', 'DDM', 'EZ'))
)
GO

CREATE TABLE Charge
(
    ID int NOT NULL IDENTITY PRIMARY KEY,
    mRID nvarchar(35) NOT NULL,
    ChargeTypeID int NOT NULL FOREIGN KEY REFERENCES ChargeType(ID),
    Name nvarchar(132) NOT NULL,
    Description nvarchar(2048) NOT NULL,
    Status TINYINT CHECK (Status >= 2 AND Status <= 4),
    StartDate bigint NOT NULL,
    EndDate bigint,
    Currency nvarchar(10) NOT NULL,
    ChargeTypeOwnerID int NOT NULL FOREIGN KEY REFERENCES MarketParticipant(ID),
    TransparentInvoicing bit NOT NULL,
    TaxIndicator bit NOT NULL,
    ResolutionTypeId int NOT NULL FOREIGN KEY REFERENCES ResolutionType(ID),
    VATPayerID int NOT NULL FOREIGN KEY REFERENCES VATPayerType(ID),
    LastUpdatedByCorrelationID nvarchar(36) NOT NULL,
    LastUpdatedByTransactionID nvarchar(100) NOT NULL,
    LastUpdatedBy nvarchar(100) NOT NULL,
    RequestDateTime bigint NOT NULL
)
GO

CREATE TABLE ChargePrice
(
    ID int NOT NULL IDENTITY PRIMARY KEY,
    ChargeId int NOT NULL FOREIGN KEY REFERENCES Charge(ID),
    Time bigint NOT NULL,
    Amount DECIMAL(14,6),
    LastUpdatedByCorrelationID nvarchar(36) NOT NULL,
    LastUpdatedByTransactionID nvarchar(100) NOT NULL,
    LastUpdatedBy nvarchar(100) NOT NULL,
    RequestDateTime bigint NOT NULL
)
GO

CREATE TABLE ValidationRuleConfiguration
(
    [Key] nvarchar(50) NOT NULL PRIMARY KEY,
    [Value] nvarchar(50) NOT NULL
)
GO