CREATE TABLE dbo.City
( 
	Id uniqueidentifier CONSTRAINT PK_City PRIMARY KEY NOT NULL,
	Name nvarchar(250) NOT NULL
)

CREATE TABLE dbo.Passport
( 
	Id uniqueidentifier CONSTRAINT PK_Passport PRIMARY KEY NOT NULL,
	Number nvarchar(250) NOT NULL
)

CREATE TABLE dbo.Person
(
	Id uniqueidentifier CONSTRAINT PK_Person PRIMARY KEY NOT NULL,
	CityId uniqueidentifier CONSTRAINT FK_City FOREIGN KEY REFERENCES dbo.City ([Id]) NULL,
	Name nvarchar(250) NOT NULL,
	Surname nvarchar(250) NOT NULL,
	Age int NOT NULL,
	PassportNumber nvarchar(250) NULL,
)