CREATE TABLE geopolitical_location(
	id INT IDENTITY(1,1),
	alpha_2 CHAR(2) NOT NULL,
	alpha_3 CHAR(3) DEFAULT(NULL),
	name VARCHAR(MAX) NOT NULL,
	official_name VARCHAR(MAX) DEFAULT(NULL),
	common_name VARCHAR(MAX) DEFAULT(NULL),
	type VARCHAR(MAX) NOT NULL,
	numeric INT DEFAULT(NULL),
	super_id INT DEFAULT(NULL),
	FOREIGN KEY (super_id) REFERENCES geopolitical_location(id),
	PRIMARY KEY(id)	
);

ALTER TABLE users ADD admin BIT NOT NULL DEFAULT(0);