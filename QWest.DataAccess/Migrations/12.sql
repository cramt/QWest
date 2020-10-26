ALTER TABLE progress_maps_locations ADD _location INT DEFAULT(NULL);

ALTER TABLE progress_maps_locations ADD FOREIGN KEY (_location) REFERENCES geopolitical_location(id) ON DELETE CASCADE;