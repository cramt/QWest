--adds users
CREATE TABLE users(
    id INT identity (1,1),
    username VARCHAR(MAX) NOT NULL,
    password_hash BINARY(36) NOT NULL,
    email VARCHAR(50) NOT NULL UNIQUE, --VARCHAR(MAX) cant be unique cause microsoft is a peice of shit
    session_cookie BINARY(20)
)