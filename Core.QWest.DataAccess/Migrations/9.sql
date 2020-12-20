ALTER TABLE users ADD profile_picture INT;

ALTER TABLE users ADD FOREIGN KEY (profile_picture) REFERENCES images(id);