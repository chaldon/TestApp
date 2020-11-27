-- Create table Brand, migrate data and update Product table

create table dbo.Brand (
	BrandId int NOT NULL IDENTITY(1, 1),
	Brand nvarchar (50) NOT NULL,
	DateCreated datetime NOT NULL DEFAULT (getdate()), -- audit columns
    DateModified datetime NOT NULL  DEFAULT (getdate()),
	PRIMARY KEY (BrandId)
);

INSERT INTO dbo.Brand (Brand) SELECT DISTINCT Brand FROM dbo.Product;

ALTER TABLE dbo.Product ADD BrandId int;

UPDATE dbo.Product SET Product.BrandId = Brand.BrandId FROM dbo.Brand JOIN dbo.Product ON dbo.Product.Brand = dbo.Brand.Brand;

ALTER TABLE dbo.Product ALTER COLUMN BrandId int NOT NULL;

ALTER TABLE dbo.Product ADD FOREIGN KEY (BrandId) REFERENCES Brand(BrandId);

ALTER TABLE dbo.Product DROP COLUMN Brand;

GO;

-- Set additional constraints on db level
-- enforce monthly and annualy
ALTER TABLE dbo.Product ADD CONSTRAINT Check_Term CHECK (Term IN ('monthly','annually') );

-- terms can't be null
ALTER TABLE dbo.Offer ALTER COLUMN NumberOfTerms int NOT NULL; 

-- Modify table Order
ALTER TABLE dbo.[Order] ADD Cancelled bit NOT NULL DEFAULT 0;
ALTER TABLE dbo.[Order] ADD Reason nvarchar(50); --Cancellation reason
ALTER TABLE dbo.[Order] ADD Paid bit NOT NULL DEFAULT 0;

GO;


CREATE PROCEDURE dbo.CreateOrder  @OfferId int,  @StartDate datetime, @CustomerId int
AS 
BEGIN
	DECLARE	@term nvarchar(50);
	DECLARE	@numberOfTerms int;

	SET NOCOUNT ON; 
	
	SELECT @term = Term, @numberOfTerms = NumberOfTerms FROM dbo.Offer JOIN dbo.Product ON dbo.Offer.ProductId = dbo.Product.ProductId WHERE OfferId = @OfferId AND Product.IsActive = 1;
	
	INSERT INTO dbo.[Order] (OfferId, StartDate, EndDate, CustomerId)
		OUTPUT Inserted.OrderId, Inserted.OfferId, Inserted.StartDate, Inserted.EndDate, Inserted.CustomerId, Inserted.Cancelled, Inserted.Paid, Inserted.Reason 
		VALUES (@OfferId, @StartDate, CASE WHEN (@term = 'monthly') THEN DATEADD( month, @numberOfTerms, @StartDate) ELSE DATEADD( year, @numberOfTerms, @StartDate) END, @CustomerId)

END;

GO;