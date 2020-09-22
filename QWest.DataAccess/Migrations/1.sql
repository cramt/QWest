--adds users
CREATE TABLE users(
    id INT identity (1,1),
    username VARCHAR(MAX) NOT NULL,
    password_hash BINARY(36) NOT NULL,
)