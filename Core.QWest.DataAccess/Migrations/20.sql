ALTER TABLE posts
ALTER COLUMN users_id INT NULL;

ALTER TABLE posts
ADD groups_id INT NULL;

ALTER TABLE posts
ADD FOREIGN KEY (groups_id) REFERENCES groups(id);