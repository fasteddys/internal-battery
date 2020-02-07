using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class integrationtestcleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.02.06 - Bill Koenig - Created
</remarks>
<description>
Undeletes (sets IsDeleted = 0) on all records whose unique identifier matches the parameter supplied
</ description >
< example >
EXEC[dbo].[System_Undelete_ObjectByGuid] @ObjectIdentifier = ''D4174402 - 686C - 4D16 - AD03 - FF81ECBB01AD''
</ example >
*/
CREATE PROCEDURE[dbo].[System_Undelete_ObjectByGuid](
    @ObjectIdentifier UNIQUEIDENTIFIER
)
AS
BEGIN

    IF(@ObjectIdentifier != ''00000000 - 0000 - 0000 - 0000 - 000000000001'' AND @ObjectIdentifier != ''00000000 - 0000 - 0000 - 0000 - 000000000000'')

    BEGIN

        DECLARE @sql NVARCHAR(MAX);
            SELECT @sql = (
                SELECT N''UPDATE '' + QUOTENAME(t.TABLE_SCHEMA) + ''.'' + QUOTENAME(t.TABLE_NAME) + '' SET IsDeleted = 0 WHERE '' + QUOTENAME(c.COLUMN_NAME) + '' = '''''' + CONVERT(VARCHAR(36), @ObjectIdentifier) + ''''''; ''

            FROM INFORMATION_SCHEMA.Columns c

            INNER JOIN(
                SELECT t1.TABLE_CATALOG, t1.TABLE_NAME, t1.TABLE_SCHEMA, t1.TABLE_TYPE
                FROM INFORMATION_SCHEMA.Columns c1

                INNER JOIN INFORMATION_SCHEMA.Tables t1 ON c1.TABLE_NAME = t1.TABLE_NAME AND t1.TABLE_TYPE = ''BASE TABLE''

                WHERE c1.COLUMN_NAME = ''IsDeleted''

                AND t1.TABLE_SCHEMA NOT IN(''Hangfire'', ''Marketing'')
                -- ignore temporary job tables

                AND t1.TABLE_NAME NOT LIKE '' %[_] % '') t ON c.TABLE_NAME = t.TABLE_NAME AND t.TABLE_TYPE = ''BASE TABLE''

            WHERE DATA_TYPE = ''uniqueidentifier''

            AND c.COLUMN_NAME NOT IN(''CreateGuid'', ''ModifyGuid'')

            FOR XML PATH('''')
		)
		EXEC sp_executesql @sql;
    END
END')");

            migrationBuilder.Sql(@"EXEC('/*
<remarks>
2020.01.31 - Bill Koenig - Created
2020.02.06 - Bill Koenig - Ignore identifiers which could trigger us to delete entities that were not created correctly
</remarks>
<description>
Deletes all records whose unique identifier matches the parameter supplied
</description>
<example>
EXEC [dbo].[System_Delete_ObjectByGuid] @ObjectIdentifier = ''D4174402-686C-4D16-AD03-FF81ECBB01AD''
</example>
*/
CREATE PROCEDURE [dbo].[System_Delete_ObjectByGuid] (
    @ObjectIdentifier UNIQUEIDENTIFIER
)
AS
BEGIN 
	IF(@ObjectIdentifier != ''00000000-0000-0000-0000-000000000001'' AND @ObjectIdentifier != ''00000000-0000-0000-0000-000000000000'')
	BEGIN
		DECLARE @sql NVARCHAR(MAX);
		SELECT @sql = (
			SELECT N''DELETE FROM '' + QUOTENAME(t.TABLE_SCHEMA) + ''.'' + QUOTENAME(t.TABLE_NAME) + '' WHERE '' + QUOTENAME(c.COLUMN_NAME) + '' = '''''' + CONVERT(VARCHAR(36), @ObjectIdentifier) + ''''''; ''
			FROM INFORMATION_SCHEMA.Columns c
			INNER JOIN (
				SELECT t1.TABLE_CATALOG, t1.TABLE_NAME, t1.TABLE_SCHEMA, t1.TABLE_TYPE
				FROM INFORMATION_SCHEMA.Columns c1
				INNER JOIN INFORMATION_SCHEMA.Tables t1 ON c1.TABLE_NAME = t1.TABLE_NAME AND t1.TABLE_TYPE = ''BASE TABLE''
				WHERE c1.COLUMN_NAME = ''IsDeleted'' 
				AND t1.TABLE_SCHEMA NOT IN (''Hangfire'', ''Marketing'')
				-- ignore temporary job tables
				AND t1.TABLE_NAME NOT LIKE ''%[_]%'') t ON c.TABLE_NAME = t.TABLE_NAME AND t.TABLE_TYPE = ''BASE TABLE''
			WHERE DATA_TYPE = ''uniqueidentifier''
			AND c.COLUMN_NAME NOT IN (''CreateGuid'', ''ModifyGuid'')
			FOR XML PATH('''')
		)
		EXEC sp_executesql @sql;
	END
END')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
