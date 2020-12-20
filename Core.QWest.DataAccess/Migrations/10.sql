CREATE TABLE geopolitical_location(
	id INT IDENTITY(1,1),
	alpha_2 CHAR(2) NOT NULL,
	alpha_3 CHAR(3) DEFAULT(NULL),
	name VARCHAR(MAX) NOT NULL,
	names VARCHAR(MAX) NOT NULL,
	super_id INT DEFAULT(NULL),
	region VARCHAR(MAX) DEFAULT(NULL),
	sub_region VARCHAR(MAX) DEFAULT(NULL),
	intermediate_region VARCHAR(MAX) DEFAULT(NULL),
	region_code INT DEFAULT(NULL),
	sub_region_code INT DEFAULT(NULL),
	intermediate_region_code INT DEFAULT(NULL),
	FOREIGN KEY (super_id) REFERENCES geopolitical_location(id),
	PRIMARY KEY(id)	
);