update posts set location = dbo.Temp(location_id);

alter table posts drop column location_id;