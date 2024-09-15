DROP TABLE "Person";
DROP TABLE "City";
DROP TABLE "Passport";

CREATE TABLE IF NOT EXISTS "City"
( 
	"Id" uuid CONSTRAINT "PK_City" PRIMARY KEY NOT NULL DEFAULT gen_random_uuid(),
	"Name" varchar(250) NOT NULL
);
ALTER TABLE "City"
    OWNER TO db_user;

CREATE TABLE IF NOT EXISTS "Passport"
( 
	"Id" uuid CONSTRAINT "PK_Passport" PRIMARY KEY NOT NULL DEFAULT gen_random_uuid(),
	"Number" varchar(250) NOT NULL
);
ALTER TABLE "Passport"
    OWNER TO db_user;
	
CREATE TABLE IF NOT EXISTS "Person"
(
	"Id" uuid CONSTRAINT "PK_Person" PRIMARY KEY NOT NULL DEFAULT gen_random_uuid(),
	"CityId" uuid CONSTRAINT "FK_City" REFERENCES "City" ("Id") NULL,
	"PassportId" uuid CONSTRAINT "FK_Passport" REFERENCES "Passport" ("Id") NULL,
	"Name" varchar(250) NOT NULL,
	"Surname" varchar(250) NOT NULL,
	"Age" int NOT NULL
);
ALTER TABLE "Person"
    OWNER TO db_user;