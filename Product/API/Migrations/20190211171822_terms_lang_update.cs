using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class terms_lang_update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("RebateType", "RebateTypeId", 1, "Terms", "Offer only eligible to original email recipient, users forwarding the email or related URL not eligible, must sign up, login and purchase the designated course in this offer to CareerCircle using the same email address.   Invitee's must sign up and purchase the designated course before March 1, 2019 . Course price of $249 can only be refunded to same payment card used for purchase once verified they have completed the course within the specified timeframe and have been placed into a contracting or other employment by an Allegis Group Company within 180 days of course enrollment.  Refunds to be issued within 30 days of all terms and conditions having been met.");
            migrationBuilder.UpdateData("RebateType", "RebateTypeId", 2, "Terms", "Offer only eligible to original email recipient, users forwarding the email or related URL not eligible, must sign up, login and purchase the designated course in this offer to CareerCircle using the same email address.   Invitee's must sign up and purchase the designated course before March 1, 2019 and complete the course within 60 days of the purchase date.  Course price of $249.00 can only be refunded to same payment card used for purchase once verified they have completed the course within the specified timeframe.  Refunds to be issued within 30 days of all terms and conditions having been met.");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
