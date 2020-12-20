CREATE FUNCTION dbo.Temp(@alpha_2_arg VARCHAR(MAX)) RETURNS INT
AS
BEGIN
   DECLARE @alpha_2s TABLE(alpha2 CHAR(2), id INT IDENTITY);
   DECLARE @alpha_2 CHAR(2);
   DECLARE @alpha_2_id INT;
   DECLARE @curr_super INT;
   SET @curr_super = NULL;

   INSERT INTO @alpha_2s (alpha2) SELECT CAST(t.value as CHAR(2)) as alpha2 FROM STRING_SPLIT(@alpha_2_arg, '-') T;

   WHILE (SELECT COUNT(*) FROM @alpha_2s) > 0
   BEGIN
      SELECT TOP 1 @alpha_2 = alpha2, @alpha_2_id = id from @alpha_2s;
	  DELETE FROM @alpha_2s WHERE id = @alpha_2_id;
	  SET @curr_super = (SELECT id FROM geopolitical_location WHERE alpha_2 = @alpha_2 AND (super_id = @curr_super OR (super_id IS NULL AND @curr_super IS NULL)));
   END

   return @curr_super;
END;