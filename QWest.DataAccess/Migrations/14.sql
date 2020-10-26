ALTER TABLE posts ADD location INT DEFAULT(NULL);

ALTER TABLE posts ADD FOREIGN KEY (location) REFERENCES geopolitical_location(id) ON DELETE CASCADE;