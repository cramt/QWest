CREATE FUNCTION dbo.FetchGeopoliticalLocation(@location_id INT) RETURNS @values TABLE (
	id INT NOT NULL,
	alpha_2 CHAR(2) NOT NULL,
	alpha_3 CHAR(3),
	name VARCHAR(MAX) NOT NULL,
	names VARCHAR(MAX) NOT NULL,
	official_name VARCHAR(MAX),
	common_name VARCHAR(MAX),
	type VARCHAR(MAX) NOT NULL,
	numeric INT,
	super_id INT
)
AS
BEGIN
	DECLARE @curr_super_id INT;
	DECLARE @curr TABLE (
		id INT NOT NULL,
		alpha_2 CHAR(2) NOT NULL,
		alpha_3 CHAR(3),
		name VARCHAR(MAX) NOT NULL,
		names VARCHAR(MAX) NOT NULL,
		official_name VARCHAR(MAX),
		common_name VARCHAR(MAX),
		type VARCHAR(MAX) NOT NULL,
		numeric INT,
		super_id INT
	);
	DECLARE @temp TABLE (
		id INT NOT NULL,
		alpha_2 CHAR(2) NOT NULL,
		alpha_3 CHAR(3),
		name VARCHAR(MAX) NOT NULL,
		names VARCHAR(MAX) NOT NULL,
		official_name VARCHAR(MAX),
		common_name VARCHAR(MAX),
		type VARCHAR(MAX) NOT NULL,
		numeric INT,
		super_id INT
	);
	SET @curr_super_id = (SELECT super_id FROM geopolitical_location WHERE id = @location_id);

	WHILE (SELECT COUNT(*) FROM geopolitical_location where id = @curr_super_id) > 0
	BEGIN
		INSERT INTO @values SELECT * FROM geopolitical_location where id = @curr_super_id;
		SET @curr_super_id = (SELECT super_id FROM geopolitical_location where id = @curr_super_id);
	END


	INSERT INTO @curr SELECT * FROM geopolitical_location WHERE id = @location_id;
	WHILE (SELECT COUNT(*) FROM @curr) != 0
	BEGIN
		DELETE FROM @temp;
		INSERT INTO @temp SELECT sub.id, sub.alpha_2, sub.alpha_3, sub.name, sub.names, sub.official_name, sub.common_name, sub.type, sub.numeric, sub.super_id FROM geopolitical_location sub INNER JOIN geopolitical_location super ON sub.super_id = super.id INNER JOIN @curr c ON super.id = c.id
		INSERT INTO @values SELECT * FROM @curr;
		DELETE FROM @curr;
		INSERT INTO @curr SELECT * FROM @temp;
	END
RETURN;
END