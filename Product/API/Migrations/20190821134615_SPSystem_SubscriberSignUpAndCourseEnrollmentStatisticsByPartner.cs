using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class SPSystem_SubscriberSignUpAndCourseEnrollmentStatisticsByPartner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            EXEC('
           CREATE PROCEDURE [dbo].[System_SubscriberSignUpAndCourseEnrollmentStatisticsByPartner] 
            @StartDate DATETIME
	        ,@EndDate DATETIME
            AS
            BEGIN
                         ;WITH 
                            SignUp_CTE
                            AS 
                            (
                                Select p.Name As PartnerName, COUNT(*) AS SignUpCount, p.PartnerId
                               from Partner p
								inner join GroupPartner gp on p.PartnerId=gp.PartnerId
								inner join [Group] g on gp.GroupId=g.GroupId
								inner join SubscriberGroup sg on g.GroupId=sg.GroupId
								inner join Subscriber s on sg.SubscriberId=s.SubscriberId
                                where s.IsDeleted=0 and sg.IsDeleted=0 and g.IsDeleted=0 and p.IsDeleted=0 and gp.IsDeleted=0 and sg.CreateDate >= @StartDate and sg.CreateDate <= @EndDate
                                Group by p.Name,p.PartnerId
                            ), Enrollment_CTE AS
                            (
                                Select p.Name As PartnerName, COUNT(*) AS Enrollments, p.PartnerId
                               from Partner p
								inner join GroupPartner gp on p.PartnerId=gp.PartnerId
								inner join [Group] g on gp.GroupId=g.GroupId
								inner join SubscriberGroup sg on g.GroupId=sg.GroupId
								inner join Subscriber s on sg.SubscriberId=s.SubscriberId
                                join Enrollment e on s.SubscriberId=e.SubscriberId
                                where s.IsDeleted=0 and sg.IsDeleted=0 and g.IsDeleted=0 and p.IsDeleted=0 and e.IsDeleted=0 and gp.IsDeleted=0 and sg.CreateDate >= @StartDate and sg.CreateDate <= @EndDate
                                Group by p.PartnerId, p.Name
                            )

                        Select scte.PartnerName, ISNULL(scte.SignUpCount,0) as SubscriberCount,ISNULL(ecte.Enrollments,0) as EnrollmentCount 
                        from SignUp_CTE scte
                        Left join Enrollment_CTE ecte on scte.PartnerId=ecte.PartnerId
            END
            ')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[System_SubscriberSignUpAndCourseEnrollmentStatisticsByPartner]");
        }
    }
}
