using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UpDiddyApi.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Badge",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    BadgeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BadgeGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: true),
                    EndDate = table.Column<DateTime>(nullable: true),
                    Points = table.Column<int>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Hidden = table.Column<bool>(nullable: true),
                    SortOrder = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badge", x => x.BadgeId);
                });

            migrationBuilder.CreateTable(
                name: "BadgeCourse",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    BadgeCourseId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BadgeCourseGuid = table.Column<Guid>(nullable: true),
                    CourseId = table.Column<string>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeCourse", x => x.BadgeCourseId);
                });

            migrationBuilder.CreateTable(
                name: "BadgeEarned",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    BadgeEarnedId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BadgeEarnedGuid = table.Column<Guid>(nullable: true),
                    BadgeId = table.Column<string>(nullable: false),
                    SubscriberId = table.Column<string>(nullable: false),
                    DateEarned = table.Column<DateTime>(nullable: false),
                    PointValue = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeEarned", x => x.BadgeEarnedId);
                });

            migrationBuilder.CreateTable(
                name: "BadgeSet",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    BadgeSetId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BadgeSetGuid = table.Column<Guid>(nullable: true),
                    BadgeId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Hidden = table.Column<bool>(nullable: true),
                    SortOrder = table.Column<int>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    Slug = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeSet", x => x.BadgeSetId);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationSubscription",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CommunicationSubscriptionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CommunicationSubscriptionGuid = table.Column<Guid>(nullable: true),
                    SubscriberId = table.Column<int>(nullable: false),
                    CommunicationTypeId = table.Column<int>(nullable: false),
                    SubscribeDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationSubscription", x => x.CommunicationSubscriptionId);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationTemplate",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CommunicationTemplateId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CommunicationTemplateGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    CommunicationTypeId = table.Column<int>(nullable: false),
                    TextTemplate = table.Column<string>(nullable: true),
                    HtmlTemplate = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationTemplate", x => x.CommunicationTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationType",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CommunicationTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CommunciationTypeGuid = table.Column<Guid>(nullable: true),
                    FrequencyInDays = table.Column<int>(nullable: false),
                    CommunicationName = table.Column<string>(nullable: false),
                    CommuncationDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationType", x => x.CommunicationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CountryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CountryGuid = table.Column<Guid>(nullable: true),
                    Code2 = table.Column<string>(nullable: false),
                    Code3 = table.Column<string>(nullable: false),
                    OfficialName = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: false),
                    Sequence = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "CourseDelivery",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseDeliveryId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseDeliveryGuid = table.Column<Guid>(nullable: true),
                    CourseId = table.Column<int>(nullable: false),
                    DeliveryMethod = table.Column<string>(nullable: false),
                    DeliveryDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseDelivery", x => x.CourseDeliveryId);
                });

            migrationBuilder.CreateTable(
                name: "CourseReview",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseReviewId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseReviewGuid = table.Column<Guid>(nullable: true),
                    CourseId = table.Column<int>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    Rating = table.Column<int>(nullable: false),
                    Review = table.Column<string>(nullable: true),
                    ApprovedToPublish = table.Column<int>(nullable: true),
                    ApprovedById = table.Column<int>(nullable: true),
                    VerifiedAttended = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseReview", x => x.CourseReviewId);
                });

            migrationBuilder.CreateTable(
                name: "CourseVariantPromoCode",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseVariantPromoCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseVariantPromoCodeGuid = table.Column<Guid>(nullable: true),
                    CourseVariantId = table.Column<int>(nullable: false),
                    PromoCodeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVariantPromoCode", x => x.CourseVariantPromoCodeId);
                });

            migrationBuilder.CreateTable(
                name: "CourseVariantType",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseVariantTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseVariantGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVariantType", x => x.CourseVariantTypeId);
                });

            migrationBuilder.CreateTable(
                name: "EducationLevel",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EducationLevelId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EducationLevelGuid = table.Column<Guid>(nullable: true),
                    Level = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationLevel", x => x.EducationLevelId);
                });

            migrationBuilder.CreateTable(
                name: "EnrollmentLog",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EnrollmentLogId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EnrollmentLogGuid = table.Column<Guid>(nullable: false),
                    EnrollmentTime = table.Column<DateTime>(nullable: false),
                    SubscriberGuid = table.Column<Guid>(nullable: false),
                    CourseGuid = table.Column<Guid>(nullable: false),
                    EnrollmentGuid = table.Column<Guid>(nullable: false),
                    CourseCost = table.Column<decimal>(nullable: false),
                    PromoApplied = table.Column<decimal>(nullable: false),
                    EnrollmentVendorPaymentStatusId = table.Column<int>(nullable: false),
                    EnrollmentVendorInvoicePaymentYear = table.Column<int>(nullable: false),
                    EnrollmentVendorInvoicePaymentMonth = table.Column<int>(nullable: false),
                    CourseVariantGuid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentLog", x => x.EnrollmentLogId);
                });

            migrationBuilder.CreateTable(
                name: "EnrollmentStatus",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EnrollmentStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EnrollmentStatusGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentStatus", x => x.EnrollmentStatusId);
                });

            migrationBuilder.CreateTable(
                name: "Gender",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    GenderId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GenderGuid = table.Column<Guid>(nullable: true),
                    SexualIdentification = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gender", x => x.GenderId);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    NewsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NewsGuid = table.Column<Guid>(nullable: true),
                    Headline = table.Column<string>(nullable: false),
                    SubText = table.Column<string>(nullable: false),
                    DateActive = table.Column<DateTime>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: true),
                    ExternalLink = table.Column<string>(nullable: true),
                    NewsTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.NewsId);
                });

            migrationBuilder.CreateTable(
                name: "NewsType",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    NewsTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NewsTypeGuid = table.Column<Guid>(nullable: true),
                    NewsClassification = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsType", x => x.NewsTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    PaymentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PaymentGuid = table.Column<Guid>(nullable: true),
                    PaymentNonce = table.Column<string>(nullable: true),
                    PaymentValue = table.Column<decimal>(nullable: false),
                    PaymentCurrencyType = table.Column<string>(nullable: true),
                    PaymentDate = table.Column<DateTime>(nullable: false),
                    PaymentStatus = table.Column<int>(nullable: false),
                    PaymentProcessorId = table.Column<int>(nullable: false),
                    EnrollmentGuid = table.Column<Guid>(nullable: false),
                    PaymentBatchId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.PaymentId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentBatch",
                columns: table => new
                {
                    PaymentBatchId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PaymentBatchGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentBatch", x => x.PaymentBatchId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentProcessor",
                columns: table => new
                {
                    PaymentProcessorId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PaymentProcessorGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentProcessor", x => x.PaymentProcessorId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentStatus",
                columns: table => new
                {
                    PaymentStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PaymentStatusGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatus", x => x.PaymentStatusId);
                });

            migrationBuilder.CreateTable(
                name: "PromoType",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PromoTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PromoTypeGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoType", x => x.PromoTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Rebate",
                columns: table => new
                {
                    RebateId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RebateGuid = table.Column<Guid>(nullable: true),
                    EnrollmentId = table.Column<int>(nullable: false),
                    RebateAmount = table.Column<decimal>(nullable: false),
                    RebateIssued = table.Column<int>(nullable: false),
                    RebateIssueDate = table.Column<DateTime>(nullable: false),
                    RebateIssueStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rebate", x => x.RebateId);
                });

            migrationBuilder.CreateTable(
                name: "RedemptionStatus",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    RedemptionStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RedemptionStatusGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedemptionStatus", x => x.RedemptionStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ReportEnrollmentByVendors",
                columns: table => new
                {
                    ReportEnrollmentByVendorId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ReportEnrollmentByVendorGuid = table.Column<Guid>(nullable: true),
                    Month = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: false),
                    EnrollmentsCompleted = table.Column<int>(nullable: false),
                    VendorId = table.Column<int>(nullable: false),
                    TotalRevenueIn = table.Column<decimal>(nullable: false),
                    TotalSplitOut = table.Column<decimal>(nullable: false),
                    CoursesCompletedCount = table.Column<int>(nullable: false),
                    CoursesDroppedCount = table.Column<int>(nullable: false),
                    TotalRefundsIssued = table.Column<decimal>(nullable: false),
                    PromoCodesRedeemedCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportEnrollmentByVendors", x => x.ReportEnrollmentByVendorId);
                });

            migrationBuilder.CreateTable(
                name: "Subscriber",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubscriberGuid = table.Column<Guid>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: false),
                    DateOfBirth = table.Column<DateTime>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    GenderId = table.Column<int>(nullable: true),
                    EducationLevelId = table.Column<int>(nullable: true),
                    ProfileImage = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    StateId = table.Column<int>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    LinkedInUrl = table.Column<string>(nullable: true),
                    FacebookUrl = table.Column<string>(nullable: true),
                    TwitterUrl = table.Column<string>(nullable: true),
                    StackOverflowUrl = table.Column<string>(nullable: true),
                    GithubUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriber", x => x.SubscriberId);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberPromoCode",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberPromoCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubscriberPromoCodeGuid = table.Column<Guid>(nullable: true),
                    SubscriberId = table.Column<int>(nullable: false),
                    PromoCodeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberPromoCode", x => x.SubscriberPromoCodeId);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    TagId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TagGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.TagId);
                });

            migrationBuilder.CreateTable(
                name: "TagCourse",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    TagCourseId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TagCourseGuid = table.Column<Guid>(nullable: true),
                    TagId = table.Column<int>(nullable: false),
                    CourseId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagCourse", x => x.TagCourseId);
                });

            migrationBuilder.CreateTable(
                name: "TagTopic",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    TagTopicId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TagTopicGuid = table.Column<Guid>(nullable: true),
                    TagId = table.Column<int>(nullable: false),
                    TopicId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTopic", x => x.TagTopicId);
                });

            migrationBuilder.CreateTable(
                name: "Topic",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    TopicId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TopicGuid = table.Column<Guid>(nullable: true),
                    Slug = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DesktopImage = table.Column<string>(nullable: true),
                    TabletImage = table.Column<string>(nullable: true),
                    MobileImage = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: true),
                    Hidden = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topic", x => x.TopicId);
                });

            migrationBuilder.CreateTable(
                name: "Vendor",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    VendorId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    VendorGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    LoginUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendor", x => x.VendorId);
                });

            migrationBuilder.CreateTable(
                name: "VendorPromoCode",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    VendorPromoCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    VendorPromoCodeGuid = table.Column<Guid>(nullable: true),
                    PromoCodeId = table.Column<int>(nullable: false),
                    VendorId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorPromoCode", x => x.VendorPromoCodeId);
                });

            migrationBuilder.CreateTable(
                name: "VendorStudentLogin",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    VendorStudentLoginId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    VendorId = table.Column<int>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    VendorLogin = table.Column<string>(nullable: true),
                    RegistrationUrl = table.Column<string>(nullable: true),
                    LastLoginDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorStudentLogin", x => x.VendorStudentLoginId);
                });

            migrationBuilder.CreateTable(
                name: "VendorTermsOfService",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    VendorTermsOfServiceId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    VendorTermsOfServiceGuid = table.Column<Guid>(nullable: true),
                    VendorId = table.Column<int>(nullable: false),
                    DateEffective = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorTermsOfService", x => x.VendorTermsOfServiceId);
                });

            migrationBuilder.CreateTable(
                name: "WozCourseEnrollment",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    WozCourseEnrollmentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    WozEnrollmentId = table.Column<int>(nullable: false),
                    SectionId = table.Column<int>(nullable: false),
                    EnrollmentStatus = table.Column<int>(nullable: false),
                    ExeterId = table.Column<int>(nullable: false),
                    EnrollmentDateUTC = table.Column<long>(nullable: false),
                    EnrollmentGuid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WozCourseEnrollment", x => x.WozCourseEnrollmentId);
                });

            migrationBuilder.CreateTable(
                name: "WozCourseSection",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    WozCourseSectionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseCode = table.Column<string>(nullable: true),
                    Section = table.Column<int>(nullable: false),
                    Month = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WozCourseSection", x => x.WozCourseSectionId);
                });

            migrationBuilder.CreateTable(
                name: "WozTermsOfService",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    WozTermsOfServiceId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DocumentId = table.Column<int>(nullable: false),
                    TermsOfService = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WozTermsOfService", x => x.WozTermsOfServiceId);
                });

            migrationBuilder.CreateTable(
                name: "WozTransactionLog",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    WozTransactionLogId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EndPoint = table.Column<string>(nullable: true),
                    InputParameters = table.Column<string>(nullable: true),
                    ResponseJson = table.Column<string>(nullable: true),
                    WozResponseJson = table.Column<string>(nullable: true),
                    EnrollmentGuid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WozTransactionLog", x => x.WozTransactionLogId);
                });

            migrationBuilder.CreateTable(
                name: "State",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    StateId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    StateGuid = table.Column<Guid>(nullable: true),
                    Code = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    CountryId = table.Column<int>(nullable: false),
                    Sequence = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_State", x => x.StateId);
                    table.ForeignKey(
                        name: "FK_State_Country_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromoCode",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PromoCodeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PromoCodeGuid = table.Column<Guid>(nullable: true),
                    Code = table.Column<string>(nullable: false),
                    PromoStartDate = table.Column<DateTime>(nullable: false),
                    PromoEndDate = table.Column<DateTime>(nullable: false),
                    PromoTypeId = table.Column<int>(nullable: false),
                    PromoValueFactor = table.Column<decimal>(nullable: false),
                    PromoName = table.Column<string>(nullable: false),
                    PromoDescription = table.Column<string>(nullable: true),
                    NumberOfRedemptions = table.Column<int>(nullable: false, defaultValue: 0),
                    MaxAllowedNumberOfRedemptions = table.Column<int>(nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCode", x => x.PromoCodeId);
                    table.ForeignKey(
                        name: "FK_PromoCode_PromoType_PromoTypeId",
                        column: x => x.PromoTypeId,
                        principalTable: "PromoType",
                        principalColumn: "PromoTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LinkedInToken",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    LinkedInTokenId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubscriberId = table.Column<int>(nullable: false),
                    AccessToken = table.Column<string>(nullable: true),
                    AccessTokenExpiry = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkedInToken", x => x.LinkedInTokenId);
                    table.ForeignKey(
                        name: "FK_LinkedInToken_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberProfileStagingStore",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    SubscriberProfileStagingStoreId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubscriberId = table.Column<int>(nullable: false),
                    ProfileData = table.Column<string>(nullable: true),
                    ProfileSource = table.Column<string>(nullable: true),
                    ProfileFormat = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberProfileStagingStore", x => x.SubscriberProfileStagingStoreId);
                    table.ForeignKey(
                        name: "FK_SubscriberProfileStagingStore_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseGuid = table.Column<Guid>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Slug = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    TopicId = table.Column<int>(nullable: true),
                    DesktopImage = table.Column<string>(nullable: true),
                    TabletImage = table.Column<string>(nullable: true),
                    MobileImage = table.Column<string>(nullable: true),
                    VendorId = table.Column<int>(nullable: false),
                    SortOrder = table.Column<int>(nullable: true),
                    CourseDeliveryId = table.Column<int>(nullable: true),
                    Hidden = table.Column<int>(nullable: true),
                    VideoUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.CourseId);
                    table.ForeignKey(
                        name: "FK_Course_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendor",
                        principalColumn: "VendorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseVariant",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    CourseVariantId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseVariantGuid = table.Column<Guid>(nullable: true),
                    Price = table.Column<decimal>(nullable: false),
                    CourseVariantTypeId = table.Column<int>(nullable: false),
                    CourseId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVariant", x => x.CourseVariantId);
                    table.ForeignKey(
                        name: "FK_CourseVariant_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseVariant_CourseVariantType_CourseVariantTypeId",
                        column: x => x.CourseVariantTypeId,
                        principalTable: "CourseVariantType",
                        principalColumn: "CourseVariantTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    EnrollmentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EnrollmentGuid = table.Column<Guid>(nullable: true),
                    CourseId = table.Column<int>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    DateEnrolled = table.Column<DateTime>(nullable: false),
                    PricePaid = table.Column<decimal>(nullable: false),
                    PercentComplete = table.Column<int>(nullable: false),
                    IsRetake = table.Column<int>(nullable: true),
                    CompletionDate = table.Column<DateTime>(nullable: true),
                    DroppedDate = table.Column<DateTime>(nullable: true),
                    EnrollmentStatusId = table.Column<int>(nullable: false),
                    TermsOfServiceFlag = table.Column<int>(nullable: true),
                    SectionStartTimestamp = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollment", x => x.EnrollmentId);
                    table.ForeignKey(
                        name: "FK_Enrollment_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollment_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromoCodeRedemption",
                columns: table => new
                {
                    IsDeleted = table.Column<int>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: true),
                    CreateGuid = table.Column<Guid>(nullable: false),
                    ModifyGuid = table.Column<Guid>(nullable: true),
                    PromoCodeRedemptionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PromoCodeRedemptionGuid = table.Column<Guid>(nullable: false),
                    RedemptionDate = table.Column<DateTime>(nullable: true),
                    ValueRedeemed = table.Column<decimal>(nullable: false),
                    RedemptionNotes = table.Column<string>(nullable: true),
                    RedemptionStatusId = table.Column<int>(nullable: false),
                    PromoCodeId = table.Column<int>(nullable: false),
                    SubscriberId = table.Column<int>(nullable: false),
                    CourseVariantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCodeRedemption", x => x.PromoCodeRedemptionId);
                    table.ForeignKey(
                        name: "FK_PromoCodeRedemption_CourseVariant_CourseVariantId",
                        column: x => x.CourseVariantId,
                        principalTable: "CourseVariant",
                        principalColumn: "CourseVariantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromoCodeRedemption_PromoCode_PromoCodeId",
                        column: x => x.PromoCodeId,
                        principalTable: "PromoCode",
                        principalColumn: "PromoCodeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromoCodeRedemption_RedemptionStatus_RedemptionStatusId",
                        column: x => x.RedemptionStatusId,
                        principalTable: "RedemptionStatus",
                        principalColumn: "RedemptionStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromoCodeRedemption_Subscriber_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscriber",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PromoType",
                columns: new[] { "PromoTypeId", "CreateDate", "CreateGuid", "Description", "IsDeleted", "ModifyDate", "ModifyGuid", "Name", "PromoTypeGuid" },
                values: new object[,]
                {
                    { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d5533b5c-6c87-4c48-b9be-d6ffb5532a4c"), "This type indicates that the PromoValueFactor is the dollar amount that should be subtracted from the course cost.", 0, null, null, "Dollar Amount", new Guid("1ddb91f6-a6e5-4c01-a020-1dea0ab77e95") },
                    { 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d5533b5c-6c87-4c48-b9be-d6ffb5532a4c"), "This type indicates that the the course cost should be reduced by the percentage value of the PromoValueFactor.", 0, null, null, "Percent Off", new Guid("1ddb91f6-a6e5-4c01-a020-1dea0ab77e95") }
                });

            migrationBuilder.InsertData(
                table: "RedemptionStatus",
                columns: new[] { "RedemptionStatusId", "CreateDate", "CreateGuid", "IsDeleted", "ModifyDate", "ModifyGuid", "Name", "RedemptionStatusGuid" },
                values: new object[,]
                {
                    { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d5533b5c-6c87-4c48-b9be-d6ffb5532a4c"), 0, null, null, "In Process", new Guid("1fe97cde-3a2d-42f1-8b8d-42824367020b") },
                    { 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d5533b5c-6c87-4c48-b9be-d6ffb5532a4c"), 0, null, null, "Completed", new Guid("1fe97cde-3a2d-42f1-8b8d-42824367020b") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Course_VendorId",
                table: "Course",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVariant_CourseId",
                table: "CourseVariant",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVariant_CourseVariantTypeId",
                table: "CourseVariant",
                column: "CourseVariantTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CourseId",
                table: "Enrollment",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_SubscriberId",
                table: "Enrollment",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkedInToken_SubscriberId",
                table: "LinkedInToken",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCode_PromoTypeId",
                table: "PromoCode",
                column: "PromoTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_CourseVariantId",
                table: "PromoCodeRedemption",
                column: "CourseVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_PromoCodeId",
                table: "PromoCodeRedemption",
                column: "PromoCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_RedemptionStatusId",
                table: "PromoCodeRedemption",
                column: "RedemptionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodeRedemption_SubscriberId",
                table: "PromoCodeRedemption",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_State_CountryId",
                table: "State",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberProfileStagingStore_SubscriberId",
                table: "SubscriberProfileStagingStore",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Badge");

            migrationBuilder.DropTable(
                name: "BadgeCourse");

            migrationBuilder.DropTable(
                name: "BadgeEarned");

            migrationBuilder.DropTable(
                name: "BadgeSet");

            migrationBuilder.DropTable(
                name: "CommunicationSubscription");

            migrationBuilder.DropTable(
                name: "CommunicationTemplate");

            migrationBuilder.DropTable(
                name: "CommunicationType");

            migrationBuilder.DropTable(
                name: "CourseDelivery");

            migrationBuilder.DropTable(
                name: "CourseReview");

            migrationBuilder.DropTable(
                name: "CourseVariantPromoCode");

            migrationBuilder.DropTable(
                name: "EducationLevel");

            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "EnrollmentLog");

            migrationBuilder.DropTable(
                name: "EnrollmentStatus");

            migrationBuilder.DropTable(
                name: "Gender");

            migrationBuilder.DropTable(
                name: "LinkedInToken");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "NewsType");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "PaymentBatch");

            migrationBuilder.DropTable(
                name: "PaymentProcessor");

            migrationBuilder.DropTable(
                name: "PaymentStatus");

            migrationBuilder.DropTable(
                name: "PromoCodeRedemption");

            migrationBuilder.DropTable(
                name: "Rebate");

            migrationBuilder.DropTable(
                name: "ReportEnrollmentByVendors");

            migrationBuilder.DropTable(
                name: "State");

            migrationBuilder.DropTable(
                name: "SubscriberProfileStagingStore");

            migrationBuilder.DropTable(
                name: "SubscriberPromoCode");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "TagCourse");

            migrationBuilder.DropTable(
                name: "TagTopic");

            migrationBuilder.DropTable(
                name: "Topic");

            migrationBuilder.DropTable(
                name: "VendorPromoCode");

            migrationBuilder.DropTable(
                name: "VendorStudentLogin");

            migrationBuilder.DropTable(
                name: "VendorTermsOfService");

            migrationBuilder.DropTable(
                name: "WozCourseEnrollment");

            migrationBuilder.DropTable(
                name: "WozCourseSection");

            migrationBuilder.DropTable(
                name: "WozTermsOfService");

            migrationBuilder.DropTable(
                name: "WozTransactionLog");

            migrationBuilder.DropTable(
                name: "CourseVariant");

            migrationBuilder.DropTable(
                name: "PromoCode");

            migrationBuilder.DropTable(
                name: "RedemptionStatus");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropTable(
                name: "Subscriber");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "CourseVariantType");

            migrationBuilder.DropTable(
                name: "PromoType");

            migrationBuilder.DropTable(
                name: "Vendor");
        }
    }
}
