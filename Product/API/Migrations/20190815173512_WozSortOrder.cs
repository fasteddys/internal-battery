using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class WozSortOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE course set SortOrder = 1 WHERE Name = 'Basic Statistics'
                UPDATE course set SortOrder = 2 WHERE Name = 'Statistical Programming in R'
                UPDATE course set SortOrder = 3 WHERE Name = 'Programming Foundations - Python'
                UPDATE course set SortOrder = 4 WHERE Name = 'Data Wrangling and Visualization'
                UPDATE course set SortOrder = 5 WHERE Name = 'Intermediate Statistics'
                UPDATE course set SortOrder = 6 WHERE Name = 'Machine Learning'
                UPDATE course set SortOrder = 7 WHERE Name = 'Metrics and Data Processing'
                UPDATE course set SortOrder = 8 WHERE Name = 'Introduction to Big Data'
                UPDATE course set SortOrder = 9 WHERE Name = 'Modeling and Optimization'

                UPDATE course set SortOrder = 1 WHERE Name = 'Security Foundations'
                UPDATE course set SortOrder = 2 WHERE Name = 'Networking Foundations'
                UPDATE course set SortOrder = 3 WHERE Name = 'System Administration'
                UPDATE course set SortOrder = 4 WHERE Name = 'Network Defense'
                UPDATE course set SortOrder = 5 WHERE Name = 'Cryptography and Access Management'
                UPDATE course set SortOrder = 6 WHERE Name = 'Logging and Monitoring'
                UPDATE course set SortOrder = 7 WHERE Name = 'Threats and Vulnerabilities'

                UPDATE course set SortOrder = 1 WHERE Name = 'Coding From Scratch'
                UPDATE course set SortOrder = 2 WHERE Name = 'Front End Foundations'
                UPDATE course set SortOrder = 3 WHERE Name = 'Front End Frameworks - React'
                UPDATE course set SortOrder = 4 WHERE Name = 'Front End Frameworks - Angular'
                UPDATE course set SortOrder = 5 WHERE Name = 'Database'
                UPDATE course set SortOrder = 6 WHERE Name = 'Programming Foundations C#'
                UPDATE course set SortOrder = 7 WHERE Name = 'Programming Foundations Java'
                UPDATE course set SortOrder = 8 WHERE Name = 'Back End Foundations - JavaScript'
                UPDATE course set SortOrder = 9 WHERE Name = 'Back End Foundations - Java'
                UPDATE course set SortOrder = 10 WHERE Name = 'Back End Foundations - C#'
                UPDATE course set SortOrder = 11 WHERE Name = 'Responsive Web Design'
                UPDATE course set SortOrder = 12 WHERE Name = 'Web Security Foundations'
                UPDATE course set SortOrder = 13 WHERE Name = 'Deployment'
                UPDATE course set SortOrder = 14 WHERE Name = 'Agile Project Management'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE Course set SortOrder = null");
        }
    }
}
