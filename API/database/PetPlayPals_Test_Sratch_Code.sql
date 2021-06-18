--given a playdate ID, get all the pets that are participating in that playdate
select pp.playdate_id,p.* from playdate_pet as pp join fullPets as p on pp.pet_id = p.pet_id where playdate_id = 1


--filter


declare @permittedPersonalites as int=-1;

--filter on allowed personality types
select distinct playdate.playdate_id from playdate join playdate_personality_permitted as ppp on playdate.playdate_id = ppp.playdate_id where (((personality_id_is_permitted = 1) and (personality_id in (@permittedPetTypes))) or (-1 in (@permittedPetTypes)))


--filter on allowed pet types
declare @permittedPetTypes as int=2

select distinct playdate.playdate_id from playdate join playdate_pet_type_permitted as ppp on playdate.playdate_id = ppp.playdate_id where (((pet_type_id_is_permitted = 1) and (pet_type_id in ({0}))) or (-1 in ({0})))


select * from playdate_pet_type_permitted where playdate_id = 2

select * from playdate_personality_permitted where playdate_id = 2

declare @centerLat as float=41.5;
declare @centerLng as float=-81.5;
declare @radius as float=-1

select playdate_id,distance_km,distance_mi from (select *, (distance_km * 0.62137)as distance_mi from (select *,dbo.Haversine_km(@centerLat,@centerLng,lat,lng) as distance_km from fullPlaydate)as km) as fullPlaydate_and_distance where( (distance_km <= @radius) or (@radius =-1) )


--overwrite playdate_personality_Permitted

declare @playdateId as int=1;
declare @permitted as bit=1;
begin transaction;delete from playdate_personality_permitted where playdate_id = @playdateId;insert into playdate_personality_permitted (playdate_id,personality_id,personality_id_is_permitted) values {0};commit transaction;
--overwrite playdate_pet_type_permitted
begin transaction;delete from playdate_pet_type_permitted where playdate_id = @playdateId;insert into playdate_pet_type_permitted (playdate_id, pet_type_id, pet_type_id_is_permitted) values {0};commit transaction;

select * from fullPlaydate

select* from "user"

begin transaction;delete from playdate_pet_type_permitted where playdate_id = @playdateId;insert into playdate_pet_type_permitted (playdate_id, pet_type_id, pet_type_id_is_permitted) values @playdateId,@petTypeId0,@petTypeIdIsPermitted0;commit transaction;

select * from playdate_pet where playdate_id = @playdateId and pet_id = @petId;

delete from playdate_pet where playdate_id = @playdateId and pet_id = @petId;

select * from fullPlaydate
select * from playdate

--update playdate by ID
select * from playdate_pet
begin transaction; delete from playdate_pet where playdate_id = @playdateId; begin try insert into playdate_pet (playdate_id, pet_id) values (null,null);end try begin catch end catch; commit transaction;

insert into playdate_personality_permitted (playdate_id,personality_id,personality_id_is_permitted) values {0}

select * from playdate_pet_type_permitted