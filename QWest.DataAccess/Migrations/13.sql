update progress_maps_locations set _location = dbo.Temp(location);

alter table progress_maps_locations drop column location;

alter table progress_maps_locations alter column _location INT NOT NULL;

EXEC sp_rename 'progress_maps_locations._location', 'location', 'COLUMN';