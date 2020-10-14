CREATE TABLE password_reset_tokens(
  users_id INT NOT NULL,
  token BINARY(50) NOT NULL,
  FOREIGN KEY (users_id) REFERENCES users(id) on delete cascade
);