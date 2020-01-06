using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class cacherelatedjobskillmatrix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('
/*
<remarks>
2019.12.04 - Bill Koenig - Created
</remarks>
<description>
Updates the related job skill matrix table with data from the view
</description>
<example>
DECLARE @ErrorLine NVARCHAR(MAX)
DECLARE @ErrorMessage NVARCHAR(MAX)
DECLARE @ErrorProcedure NVARCHAR(MAX)

EXEC [dbo].[System_Cache_RelatedJobSkillMatrix] 
	@ErrorLine = @ErrorLine OUTPUT,
	@ErrorMessage = @ErrorMessage OUTPUT,
	@ErrorProcedure = @ErrorProcedure OUTPUT

SELECT @ErrorLine [ErrorLine], @ErrorMessage [ErrorMessage], @ErrorProcedure [ErrorProcedure]
</example>
*/
CREATE PROCEDURE [dbo].[System_Cache_RelatedJobSkillMatrix] (
	@ErrorLine NVARCHAR(MAX) OUTPUT,
	@ErrorMessage NVARCHAR(MAX) OUTPUT,
	@ErrorProcedure NVARCHAR(MAX) OUTPUT
)
AS
BEGIN
	
	SET NOCOUNT ON;
	BEGIN TRANSACTION;
	BEGIN TRY

		TRUNCATE TABLE dbo.RelatedJobSkillMatrix
		
		-- testing error handling routine and rollback with table truncation
		-- DECLARE @derp int = 1/0

		INSERT INTO [dbo].[RelatedJobSkillMatrix] (
			[SkillId]
			,[PopularityScore]
			,[PopularityIndex]
			,[RelatedSkillId]
			,[MatchScore]
			,[MatchIndex])
		SELECT [SkillId]
			,[PopularityScore]
			,[PopularityIndex]
			,[RelatedSkillId]
			,[MatchScore]
			,[MatchIndex]
		FROM v_RelatedJobSkillMatrix

	END TRY
	BEGIN CATCH
		SET @ErrorProcedure = ERROR_PROCEDURE()
		SET @ErrorLine = ERROR_LINE()
		SET @ErrorMessage = ERROR_MESSAGE()

		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
	END CATCH;

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION;

END
            ')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[System_Cache_RelatedJobSkillMatrix]");
        }
    }
}
