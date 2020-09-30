CREATE TABLE progress_maps_locations(
  progress_maps_id INT NOT NULL,
  location VARCHAR(MAX) NOT NULL,
  FOREIGN KEY (progress_maps_id) REFERENCES progress_maps(id) on delete cascade
);