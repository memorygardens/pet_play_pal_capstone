USE master
GO

--drop database if it exists
IF DB_ID('PetPlayPals_Test') IS NOT NULL
BEGIN
	ALTER DATABASE PetPlayPals_Test SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	DROP DATABASE PetPlayPals_Test;
END

CREATE DATABASE PetPlayPals_Test
GO

USE PetPlayPals_Test
GO

--create tables

--locations table
create table location (
	location_id int identity(1,1) not null,
	name varchar(60) null,
	address varchar(80) null,
	lat numeric(10,6) not null,
	lng numeric(10,6) not null,

	CONSTRAINT PK_location PRIMARY KEY (location_id)
)

--user table
CREATE TABLE "user" (
	user_id int IDENTITY(1,1) NOT NULL,
	username varchar(50) NOT NULL,
	password_hash varchar(200) NOT NULL,
	salt varchar(200) NOT NULL,
	user_role varchar(50) NOT NULL,
	location_id int null,
	CONSTRAINT PK_user PRIMARY KEY (user_id)
)


--populate default data
INSERT INTO "user" (username, password_hash, salt, user_role) VALUES ('user','Jg45HuwT7PZkfuKTz6IB90CtWY4=','LHxP4Xh7bN0=','user');
INSERT INTO "user" (username, password_hash, salt, user_role) VALUES ('admin','YhyGVQ+Ch69n4JMBncM4lNF/i9s=', 'Ar/aB2thQTI=','admin');

-- ######## pet stuff ############

--pet_type table
create table pet_type(
	pet_type_id int identity(1,1) NOT NULL,
	pet_type_name varchar(10) NOT NULL,

	constraint PK_pet_types primary key(pet_type_id)
)

-- populate the pet types table
insert into pet_type(pet_type_name) values 
	('Dog'),
	('Cat')

--personality table
create table personality(
	personality_id int identity(1,1) not null,
	personality_name varchar(20) not null

	constraint PK_pet_personality primary key(personality_id)
)
--populate the pet personality table
insert into personality(personality_name) values 
	('Friendly'),
	('Plays Rough'),
	('Shy'),
	('Skittish'),
	('High-energy'),
	('Reactive'),
	('Gentle')


--pets table
create table pet(
	pet_id int identity(1,1) NOT NULL,
	user_id int NOT NULL,
	pet_name varchar(30) NOT NULL,
	birthday Date NOT NULL,
	sex char NOT NULL,
	pet_type_id int not null,
	pet_breed varchar(50),
	color varchar(50) null,
	bio varchar(300),
	pet_image_url varchar(2000),


	constraint PK_pet primary key (pet_id),
	constraint FK_pet_type_id foreign key (pet_type_id) references pet_type (pet_type_id),
	constraint FK_pet_user_id foreign key (user_id) references "user" (user_id),
	constraint CHK_sex check (sex in ('M','F'))
)

--pet pictures
create table pet_image(
	pet_image_id int identity(1,1) not null,
	pet_image_data varbinary(max)not null,
	pet_id int not null,
	constraint PK_pet_image primary key (pet_image_id),
	constraint FK_pet_image_pet_id foreign key (pet_id) references pet (pet_id)
)


--personality_pet relator table
create table personality_pet(
	pet_id int not null,
	personality_id int not null,
	constraint FK_personality_pet_pet_id foreign key (pet_id) references pet (pet_id),
	constraint FK_personality_pet_personality_id foreign key (personality_id) references personality (personality_id),
	constraint UC_personality_pet unique (personality_id,pet_id)
)

-- ####### playdate stuff #########
create table playdate(
	playdate_id int identity(1,1) not null,
	start_date_time dateTime not null,
	end_date_time dateTime not null,
	user_id int not null,
	location_id int not null,
	description varchar(300),


	constraint PK_playdate primary key (playdate_id),
	constraint FK_playdate_location foreign key (location_id) references location (location_id),
	constraint FK_playdate_user_id foreign key (user_id) references "user" (user_id)
)

--playdate_pet relator table
create table playdate_pet(
	playdate_id int not null,
	pet_id int not null,
	constraint FK_playdate_id foreign key (playdate_id) references playdate (playdate_id),
	constraint FK_playdate_pet_pet_id foreign key (pet_id) references pet (pet_id),
	constraint UC_playdate_pet_playdate_id_pet_id unique (playdate_id,pet_id)
)
--playdate_personality_permitted relator table
create table playdate_personality_permitted(
	playdate_id int not null,
	personality_id int not null,
	personality_id_is_permitted bit not null, 
	constraint FK_playdate_personality_permitted_playdate_id foreign key (playdate_id) references playdate (playdate_id),
	constraint FK_playdate_personality_permitted_personality_id foreign key (personality_id) references personality (personality_id),
	constraint UC_playdate_personality_permitted_playdate_id_personality_id unique (playdate_id, personality_id)
)

create table playdate_pet_type_permitted(
	playdate_id int not null,
	pet_type_id int not null,
	pet_type_id_is_permitted bit not null, 
	constraint FK_playdate_pet_type_permitted_playdate_id foreign key (playdate_id) references playdate (playdate_id),
	constraint FK_playdate_pet_type_permitted_pet_type_id foreign key (pet_type_id) references pet_type (pet_type_id),
	constraint UC_playdate_pet_type_permitted_playdate_id_pet_type_id unique (playdate_id,pet_type_id)
)

GO
-- views. We never learned this but thats okay
create view fullPet as
select pet.*, pet_type.pet_type_name from pet join pet_type on pet.pet_type_id = pet_type.pet_type_id;
go

create view fullPlaydate as 
	select p.*,u.username, l.name as location_name, address, lat, lng from playdate as p
	join location as l on p.location_id = l.location_id
	join "user" as u on p.user_id = u.user_id;
go

create view playdateIdAndPetType as 
select distinct playdate.playdate_id ,fullPet.pet_type_id,pet_type_name  from
	playdate join playdate_pet as pp on playdate.playdate_id = pp.playdate_id
	join fullPet on pp.pet_id = fullPet.pet_id
go

--custom functions. thank you stack overflow
create function dbo.Haversine_km(@lat1 float, @long1 float, @lat2 float, @long2 float)
	returns float 
	begin
    declare @dlon float, @dlat float, @rlat1 float, @rlat2 float, @rlong1 float, @rlong2 float, @a float, @c float, @R float, @d float, @DtoR float

    select @DtoR = 0.017453293
    select @R = 6371      -- Earth radius

    select 
        @rlat1 = @lat1 * @DtoR,
        @rlong1 = @long1 * @DtoR,
        @rlat2 = @lat2 * @DtoR,
        @rlong2 = @long2 * @DtoR

    select 
        @dlon = @rlong1 - @rlong2,
        @dlat = @rlat1 - @rlat2

    select @a = power(sin(@dlat/2), 2) + cos(@rlat1) * cos(@rlat2) * power(sin(@dlon/2), 2)
    select @c = 2 * atn2(sqrt(@a), sqrt(1-@a))
    select @d = @R * @c

    return @d 
end
go
