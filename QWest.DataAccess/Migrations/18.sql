CREATE TABLE users_friendships(
	left_user_id INT NOT NULL,
	right_user_id INT NOT NULL,
	FOREIGN KEY (left_user_id) REFERENCES users(id),
	FOREIGN KEY (right_user_id) REFERENCES users(id)
);

CREATE TABLE users_friendship_requests(
	from_user_id INT NOT NULL,
	to_user_id INT NOT NULL,
	FOREIGN KEY (from_user_id) REFERENCES users(id),
	FOREIGN KEY (to_user_id) REFERENCES users(id)
);