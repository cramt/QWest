CREATE TABLE groups(
	id INT identity(1,1),
	name VARCHAR(MAX) NOT NULL,
	creation_time INT NOT NULL,
	description VARCHAR(MAX) NOT NULL DEFAULT(''),
	progress_maps_id INT NOT NULL,
	PRIMARY KEY(id),
	FOREIGN KEY (progress_maps_id) REFERENCES progress_maps(id)
);

CREATE TABLE users_groups(
	users_id INT NOT NULL,
	groups_id INT NOT NULL,
	FOREIGN KEY (users_id) REFERENCES users(id),
	FOREIGN KEY (groups_id) REFERENCES groups(id),
)