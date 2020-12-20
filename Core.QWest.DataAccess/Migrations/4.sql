--add forerign key constraints
ALTER TABLE users ADD FOREIGN KEY (progress_maps_id) REFERENCES progress_maps(id) ON DELETE CASCADE;