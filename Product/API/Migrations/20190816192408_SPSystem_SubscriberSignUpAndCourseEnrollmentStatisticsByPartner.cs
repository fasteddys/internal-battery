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
            AS
            BEGIN
                        ;WITH 
                            SignUp_CTE
                            AS 
                            (
                                Select p.Name As PartnerName, COUNT(*) AS SignUpCount, p.PartnerId
                                from Partner p
                                join [Group] g on p.PartnerId=g.PartnerId
                                join SubscriberGroup sg on g.GroupId=sg.GroupId
                                join Subscriber s on s.SubscriberId=sg.SubscriberId
                                where s.IsDeleted=0 and sg.IsDeleted=0 and g.IsDeleted=0 and p.IsDeleted=0
                                Group by p.Name,p.PartnerId
                            ), Enrollment_CTE AS
                            (
                                Select p.Name As PartnerName, COUNT(*) AS Enrollments, p.PartnerId
                                from Partner p
                                join [Group] g on p.PartnerId=g.PartnerId
                                join SubscriberGroup sg on g.GroupId=sg.GroupId
                                join Subscriber s on s.SubscriberId=sg.SubscriberId
                                join Enrollment e on s.SubscriberId=e.SubscriberId
                                where s.IsDeleted=0 and sg.IsDeleted=0 and g.IsDeleted=0 and p.IsDeleted=0 and e.IsDeleted=0
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
