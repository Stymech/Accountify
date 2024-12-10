--Existing buyers
CREATE TABLE Buyers (
    BuyerID INT PRIMARY KEY IDENTITY(1,1),
    BuyerName NVARCHAR(255) NOT NULL,
    BuyerAddress NVARCHAR(255),
    BuyerTel NVARCHAR(20),
    BuyerEmail NVARCHAR(255),
    CONSTRAINT UQ_BuyerNameAddress UNIQUE (BuyerName, BuyerAddress)
);

--Articles for Sale
CREATE TABLE Articles (
	ArticleID INT PRIMARY KEY IDENTITY(1,1),
	ArticleName NVARCHAR (100) NOT NULL,
	ArticlePrice DECIMAL(10, 2) NOT NULL,
	ArticleDescription NVARCHAR(MAX)
);

--Existing Receipts
CREATE TABLE Receipts (
    ReceiptID INT PRIMARY KEY IDENTITY(1,1),
    BuyerID INT,
    BuyerName NVARCHAR(255),
    BuyerAddress NVARCHAR(255),
    ReceiptDate DATE NOT NULL,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    CompanyInfo NVARCHAR(MAX),
    FOREIGN KEY (BuyerName, BuyerAddress) REFERENCES Buyers(BuyerName, BuyerAddress),
    FOREIGN KEY (BuyerID) REFERENCES Buyers(BuyerID)
);


--Articles included on Receipt of ReceiptID
CREATE TABLE ReceiptItems (
    ReceiptItemID INT PRIMARY KEY IDENTITY(1,1),
    ReceiptID INT REFERENCES Receipts(ReceiptID),
    ArticleID INT REFERENCES Articles(ArticleID),
    Quantity INT NOT NULL,
);

------------------------------------Populate the tables---------------------------------------------

-- Generate 10 Garden-related Articles
INSERT INTO Articles (ArticleName, ArticlePrice, ArticleDescription)
VALUES
('Wheelbarrow', 59.99, 'A sturdy steel wheelbarrow with a 6-cubic-foot capacity.'),
('Garden Hose', 24.99, 'A 50-foot garden hose with a flexible design for easy use.'),
('Shovel', 16.00, 'A durable steel shovel for gardening and landscaping.'),
('Mulch', 8.00, 'Bag of organic mulch for garden beds.'),
('Potted Plant', 14.99, 'Decorative potted plant for indoor or outdoor use.'),
('Pruning Shears', 12.49, 'High-quality pruning shears for trimming plants.'),
('Garden Rake', 19.99, 'A versatile garden rake for clearing debris.'),
('Garden Gloves', 9.99, 'Durable garden gloves for protecting your hands.'),
('Garden Trowel', 6.99, 'Handy garden trowel for planting and transplanting.'),
('Garden Fertilizer', 12.99, 'Premium garden fertilizer for healthy plant growth.');

-- Generate 1000 Buyers
INSERT INTO Buyers (BuyerName, BuyerAddress, BuyerTel, BuyerEmail)
SELECT TOP 1000
    CONCAT('Buyer', ROW_NUMBER() OVER (ORDER BY NEWID())),
    CONCAT('Address', ROW_NUMBER() OVER (ORDER BY NEWID())),
    CONCAT('Tel', ROW_NUMBER() OVER (ORDER BY NEWID())),
    CONCAT('Email', ROW_NUMBER() OVER (ORDER BY NEWID()))
FROM master.dbo.spt_values;

-- Generate 1000 Receipts with random articles
DECLARE @TotalAmount DECIMAL(10, 2)
DECLARE @CompanyInfo NVARCHAR(MAX)
SET @TotalAmount = 0

DECLARE @Counter INT
SET @Counter = 1

WHILE @Counter <= 1000  -- Generate 1000 Receipts
BEGIN
    -- Select random BuyerID
    DECLARE @BuyerID INT
    SELECT TOP 1 @BuyerID = BuyerID FROM Buyers ORDER BY NEWID()

    -- Generate random ReceiptDate within the last year
    DECLARE @ReceiptDate DATE
    SET @ReceiptDate = DATEADD(day, -CAST(RAND() * 365 AS INT), GETDATE())

    -- Generate random TotalAmount between 1 and 500
    SET @TotalAmount = CAST(RAND() * 500 AS DECIMAL(10, 2))

    -- Insert into Receipts
    INSERT INTO Receipts (BuyerID, BuyerName, BuyerAddress, ReceiptDate, TotalAmount, CompanyInfo)
    VALUES (@BuyerID,
            (SELECT BuyerName FROM Buyers WHERE BuyerID = @BuyerID),
            (SELECT BuyerAddress FROM Buyers WHERE BuyerID = @BuyerID),
            @ReceiptDate,
            @TotalAmount,
            'Cvetka d.o.o.')

    -- Increment Counter
    SET @Counter = @Counter + 1
END

-- Generate ReceiptItems for the Receipts
INSERT INTO ReceiptItems (ReceiptID, ArticleID, Quantity)
SELECT
    Receipts.ReceiptID,
    Articles.ArticleID,
    FLOOR(RAND() * 10) + 1 -- Random quantity between 1 and 10
FROM Receipts
CROSS JOIN Articles
WHERE RAND() < 0.5 -- Adjust the probability as needed
