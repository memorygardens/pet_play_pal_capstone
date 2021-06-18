--delete from all tables
delete from playdate_pet
delete from personality_pet
delete from pet_image
delete from pet
delete from playdate_personality_permitted
delete from playdate_pet_type_permitted
delete from playdate
delete from location
delete from "user"

--reset IDs
DBCC CHECKIDENT ('pet_image', RESEED, 0)  
DBCC CHECKIDENT ('pet', RESEED, 0)  
DBCC CHECKIDENT ('playdate', RESEED, 0)  
DBCC CHECKIDENT ('location', RESEED, 0) 
DBCC CHECKIDENT ('user', RESEED, 0) 

--insert a location
insert into location(name,address,lat,lng) values
	('Tech Elevator','7100 Euclid Ave #140, Cleveland, OH 44103', 41.50383000392251,-81.63868811898969),
	('Canine Meadow Dog Park', '9038 Euclid Chardon Rd, Kirtland, OH 44094', 41.578580, -81.319240),
	('affoGATO Cat Cafe', '761 Starkweather Ave, Cleveland, OH 44113', 41.477340, -81.682290)

--creating users from web app
INSERT INTO "user" (username, password_hash, salt, user_role) VALUES ('user','Jg45HuwT7PZkfuKTz6IB90CtWY4=','LHxP4Xh7bN0=','user');
INSERT INTO "user" (username, password_hash, salt, user_role) VALUES ('admin','YhyGVQ+Ch69n4JMBncM4lNF/i9s=', 'Ar/aB2thQTI=','admin');
INSERT INTO "user" (username, password_hash, salt, user_role) VALUES ('brandon', 'V0lRjxFQxgKeP+/h5IbKqnAPQoU=', 'Rz+Z8yMoqIg=', 'user');
insert into "user" (username, password_hash, salt, user_role) values ('rosa', 'EBbMAfKO8NXslvRz6GCCVmeV6ig=', 'ybDc/+RXVqs=', 'user');
insert into "user" (username, password_hash, salt, user_role) values ('paul', 'fp9wBngxMgdo2HIcts7YLBJdhyU=', 'Sqzc1pc53es=', 'user');
insert into "user" (username, password_hash, salt, user_role) values ('aklile', 'HqkX65RZf4sM+SaOwjcVhDKJgvY=', 'v7N29suiPBA=', 'user');

--insert a playdate


insert into playdate(start_date_time,end_date_time,user_id,location_id) values
	('04-20-2021 12:00:00','04-20-2021 13:00:00',5,1),
	('02-12-2021 13:30:00','02-12-2021 15:00:00',4,2),
	('11-11-2021 17:00:00','11-11-2021 19:00:00',3,3)


--insert into playdate_allowed_personalities
insert into playdate_personality_permitted(playdate_id,personality_id,personality_id_is_permitted) values
	(1,1,1),
	(1,2,0),
	(1,3,1),
	(1,5,1),
	(2,3,1),
	(2,7,1),
	(3,1,1),
	(3,7,1)

--insert into playdate_pet_type_permitted
insert into playdate_pet_type_permitted(playdate_id,pet_type_id,pet_type_id_is_permitted) values 
	(1,1,1),
	(2,1,1),
	(2,2,1),
	(3,2,1)

--insert a pet 
insert into pet(user_id, pet_name, birthday, sex, pet_type_id, pet_breed, color, bio) values (4,'Ramona', '12-02-2018', 'F', 1, 'Mix', 'Spotted', 'A goofy lass');
insert into pet(user_id, pet_name, birthday, sex, pet_type_id, pet_breed, color, bio) values (5,'Maggie', '05-12-2019', 'F', 1, 'Goldendoodle', 'Golden', 'Perfect snuggler'  );
insert into pet(user_id, pet_name, birthday, sex, pet_type_id, pet_breed, color, bio) values (6,'Carl', '01-28-2020', 'M', 1, 'Mix', 'Brown and Black', 'Loud boy');
insert into pet(user_id, pet_name, birthday, sex, pet_type_id, pet_breed, color, bio) values (3,'Hanzo', '01-15-2015', 'M', 2, 'Brown Tabby', 'Brown and Black', 'HAMzo is fat');
insert into pet(user_id, pet_name, birthday, sex, pet_type_id, pet_breed, color, bio) values (3,'Pippin', '03-07-2019', 'M', 2, 'American Shorthair', 'White and Gray', 'Catch me if you can');

--connecting pets to play dates
insert into playdate_pet(playdate_id, pet_id) Values(1,3);
insert into playdate_pet(playdate_id, pet_id) Values(1,1);
insert into playdate_pet(playdate_id, pet_id) Values(2,4);
insert into playdate_pet(playdate_id, pet_id) Values(2,2);
insert into playdate_pet(playdate_id, pet_id) Values(3,4);
insert into playdate_pet(playdate_id, pet_id) Values(3,5);


--populating pet personality
insert into personality_pet (personality_id, pet_id) values (6, 1);
insert into personality_pet (personality_id, pet_id) values (1, 1);
insert into personality_pet (personality_id, pet_id) values (1, 3);
insert into personality_pet (personality_id, pet_id) values (5, 3);
insert into personality_pet (personality_id, pet_id) values (2, 3);
insert into personality_pet (personality_id, pet_id) values (1, 4);
insert into personality_pet (personality_id, pet_id) values (1, 2);
insert into personality_pet (personality_id, pet_id) values (7, 2);
insert into personality_pet (personality_id, pet_id) values (1, 5);
