CREATE TABLE posts(
	id INT identity (1,1),
	content VARCHAR(MAX) NOT NULL,
	users_id INT NOT NULL,
	post_time INT NOT NULL,
	location_id VARCHAR(MAX),
	PRIMARY KEY (id),
	FOREIGN KEY (users_id) REFERENCES users(id)
);

CREATE TABLE images(
	id INT identity (1,1),
	image_blob VARCHAR(MAX) NOT NULL,
	PRIMARY KEY (id)
)

CREATE TABLE posts_images(
	posts_id INT NOT NULL,
	images_id INT NOT NULL,
	FOREIGN KEY(images_id) REFERENCES images(id),
	FOREIGN KEY(posts_id) REFERENCES posts(id)
)