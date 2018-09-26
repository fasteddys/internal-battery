﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UpDiddyApi.Models;

namespace UpDiddyApi.Migrations
{
    [DbContext(typeof(UpDiddyDbContext))]
    partial class UpDiddyDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.2-rtm-30932")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("UpDiddyApi.Models.Badge", b =>
                {
                    b.Property<int>("BadgeId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid?>("BadgeGuid");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<string>("Description");

                    b.Property<DateTime?>("EndDate");

                    b.Property<bool?>("Hidden");

                    b.Property<string>("Icon");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int?>("Points");

                    b.Property<string>("Slug");

                    b.Property<string>("SortOrder");

                    b.Property<DateTime?>("StartDate");

                    b.HasKey("BadgeId");

                    b.ToTable("Badge");
                });

            modelBuilder.Entity("UpDiddyApi.Models.BadgeCourse", b =>
                {
                    b.Property<int>("BadgeCourseId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid?>("BadgeCourseGuid");

                    b.Property<string>("CourseId")
                        .IsRequired();

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("Notes");

                    b.Property<int?>("SortOrder");

                    b.HasKey("BadgeCourseId");

                    b.ToTable("BadgeCourse");
                });

            modelBuilder.Entity("UpDiddyApi.Models.BadgeEarned", b =>
                {
                    b.Property<int>("BadgeEarnedId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid?>("BadgeEarnedGuid");

                    b.Property<string>("BadgeId")
                        .IsRequired();

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<DateTime>("DateEarned");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<int>("PointValue");

                    b.Property<string>("SubscriberId")
                        .IsRequired();

                    b.HasKey("BadgeEarnedId");

                    b.ToTable("BadgeEarned");
                });

            modelBuilder.Entity("UpDiddyApi.Models.BadgeSet", b =>
                {
                    b.Property<int>("BadgeSetId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("BadgeId");

                    b.Property<Guid?>("BadgeSetGuid");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<string>("Description");

                    b.Property<bool?>("Hidden");

                    b.Property<string>("Icon");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Slug");

                    b.Property<int?>("SortOrder");

                    b.HasKey("BadgeSetId");

                    b.ToTable("BadgeSet");
                });

            modelBuilder.Entity("UpDiddyApi.Models.CommunicationSubscription", b =>
                {
                    b.Property<int>("CommunicationSubscriptionId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid?>("CommunicationSubscriptionGuid");

                    b.Property<int>("CommunicationTypeId");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<DateTime>("SubscribeDate");

                    b.Property<int>("SubscriberId");

                    b.HasKey("CommunicationSubscriptionId");

                    b.ToTable("CommunicationSubscription");
                });

            modelBuilder.Entity("UpDiddyApi.Models.CommunicationTemplate", b =>
                {
                    b.Property<int>("CommunicationTemplateId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid?>("CommunicationTemplateGuid");

                    b.Property<int>("CommunicationTypeId");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<string>("HtmlTemplate");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("TextTemplate");

                    b.HasKey("CommunicationTemplateId");

                    b.ToTable("CommunicationTemplate");
                });

            modelBuilder.Entity("UpDiddyApi.Models.CommunicationType", b =>
                {
                    b.Property<int>("CommunicationTypeId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CommuncationDescription");

                    b.Property<Guid?>("CommunciationTypeGuid");

                    b.Property<string>("CommunicationName")
                        .IsRequired();

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("FrequencyInDays");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.HasKey("CommunicationTypeId");

                    b.ToTable("CommunicationType");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Course", b =>
                {
                    b.Property<int>("CourseId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Code");

                    b.Property<int?>("CourseDeliveryId");

                    b.Property<Guid?>("CourseGuid");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<string>("Description");

                    b.Property<string>("DesktopImage");

                    b.Property<int>("IsDeleted");

                    b.Property<string>("MobileImage");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<decimal?>("Price");

                    b.Property<string>("Slug")
                        .IsRequired();

                    b.Property<int?>("SortOrder");

                    b.Property<string>("TabletImage");

                    b.Property<int?>("TopicId");

                    b.Property<int?>("VendorId");

                    b.HasKey("CourseId");

                    b.ToTable("Course");
                });

            modelBuilder.Entity("UpDiddyApi.Models.CourseDelivery", b =>
                {
                    b.Property<int>("CourseDeliveryId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid?>("CourseDeliveryGuid");

                    b.Property<int>("CourseId");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<string>("DeliveryDescription");

                    b.Property<string>("DeliveryMethod")
                        .IsRequired();

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.HasKey("CourseDeliveryId");

                    b.ToTable("CourseDelivery");
                });

            modelBuilder.Entity("UpDiddyApi.Models.CoursePromoCode", b =>
                {
                    b.Property<int>("CoursePromoCodeId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CourseId");

                    b.Property<Guid?>("CoursePromoCodeGuid");

                    b.Property<int>("PromoCodeId");

                    b.HasKey("CoursePromoCodeId");

                    b.ToTable("CoursePromoCode");
                });

            modelBuilder.Entity("UpDiddyApi.Models.CourseReview", b =>
                {
                    b.Property<int>("CourseReviewId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("ApprovedById");

                    b.Property<int?>("ApprovedToPublish");

                    b.Property<int>("CourseId");

                    b.Property<Guid?>("CourseReviewGuid");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<int>("Rating");

                    b.Property<string>("Review");

                    b.Property<int>("SubscriberId");

                    b.Property<int>("VerifiedAttended");

                    b.HasKey("CourseReviewId");

                    b.ToTable("CourseReview");
                });

            modelBuilder.Entity("UpDiddyApi.Models.EducationLevel", b =>
                {
                    b.Property<int>("EducationLevelId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<Guid?>("EducationLevelGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<string>("Level")
                        .IsRequired();

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.HasKey("EducationLevelId");

                    b.ToTable("EducationLevel");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Enrollment", b =>
                {
                    b.Property<int>("EnrollmentId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("CompletionDate");

                    b.Property<int>("CourseId");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<DateTime>("DateEnrolled");

                    b.Property<DateTime?>("DroppedDate");

                    b.Property<Guid?>("EnrollmentGuid");

                    b.Property<int>("EnrollmentStatusId");

                    b.Property<int>("IsDeleted");

                    b.Property<int?>("IsRetake");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<int>("PercentComplete");

                    b.Property<decimal>("PricePaid");

                    b.Property<int>("SubscriberId");

                    b.Property<int?>("TermsOfServiceFlag");

                    b.HasKey("EnrollmentId");

                    b.HasIndex("CourseId");

                    b.HasIndex("SubscriberId");

                    b.ToTable("Enrollment");
                });

            modelBuilder.Entity("UpDiddyApi.Models.EnrollmentStatus", b =>
                {
                    b.Property<int>("EnrollmentStatusId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description");

                    b.Property<Guid?>("EnrollmentStatusGuid");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("EnrollmentStatusId");

                    b.ToTable("EnrollmentStatus");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Gender", b =>
                {
                    b.Property<int>("GenderId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<Guid?>("GenderGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("SexualIdentification")
                        .IsRequired();

                    b.HasKey("GenderId");

                    b.ToTable("Gender");
                });

            modelBuilder.Entity("UpDiddyApi.Models.News", b =>
                {
                    b.Property<int>("NewsId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Body");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<DateTime>("DateActive");

                    b.Property<string>("ExternalLink");

                    b.Property<string>("Headline")
                        .IsRequired();

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<Guid?>("NewsGuid");

                    b.Property<int>("NewsTypeId");

                    b.Property<int?>("SortOrder");

                    b.Property<string>("SubText")
                        .IsRequired();

                    b.HasKey("NewsId");

                    b.ToTable("News");
                });

            modelBuilder.Entity("UpDiddyApi.Models.NewsType", b =>
                {
                    b.Property<int>("NewsTypeId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("NewsClassification")
                        .IsRequired();

                    b.Property<Guid?>("NewsTypeGuid");

                    b.HasKey("NewsTypeId");

                    b.ToTable("NewsType");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Payment", b =>
                {
                    b.Property<int>("PaymentId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("EnrollmentId");

                    b.Property<int>("PaymentBatchId");

                    b.Property<string>("PaymentCurrencyType");

                    b.Property<DateTime>("PaymentDate");

                    b.Property<Guid?>("PaymentGuid");

                    b.Property<string>("PaymentNonce");

                    b.Property<int>("PaymentProcessorId");

                    b.Property<int>("PaymentStatus");

                    b.Property<decimal>("PaymentValue");

                    b.HasKey("PaymentId");

                    b.ToTable("Payment");
                });

            modelBuilder.Entity("UpDiddyApi.Models.PaymentBatch", b =>
                {
                    b.Property<int>("PaymentBatchId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Guid?>("PaymentBatchGuid");

                    b.Property<string>("Status");

                    b.HasKey("PaymentBatchId");

                    b.ToTable("PaymentBatch");
                });

            modelBuilder.Entity("UpDiddyApi.Models.PaymentProcessor", b =>
                {
                    b.Property<int>("PaymentProcessorId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Guid?>("PaymentProcessorGuid");

                    b.HasKey("PaymentProcessorId");

                    b.ToTable("PaymentProcessor");
                });

            modelBuilder.Entity("UpDiddyApi.Models.PaymentStatus", b =>
                {
                    b.Property<int>("PaymentStatusId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Guid?>("PaymentStatusGuid");

                    b.HasKey("PaymentStatusId");

                    b.ToTable("PaymentStatus");
                });

            modelBuilder.Entity("UpDiddyApi.Models.PromoCodes", b =>
                {
                    b.Property<int>("PromoCodesId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("PromoCode")
                        .IsRequired();

                    b.Property<Guid?>("PromoCodesGuid");

                    b.Property<string>("PromoDescription");

                    b.Property<DateTime>("PromoEndDate");

                    b.Property<string>("PromoName")
                        .IsRequired();

                    b.Property<DateTime>("PromoStartDate");

                    b.Property<int>("PromoTypeId");

                    b.Property<decimal>("PromoValueFacotr");

                    b.HasKey("PromoCodesId");

                    b.ToTable("PromoCode");
                });

            modelBuilder.Entity("UpDiddyApi.Models.PromoType", b =>
                {
                    b.Property<int>("PromoTypeId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("PromoTypeDescription");

                    b.Property<Guid?>("PromoTypeGuid");

                    b.Property<string>("PromoTypeName")
                        .IsRequired();

                    b.HasKey("PromoTypeId");

                    b.ToTable("PromoType");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Rebate", b =>
                {
                    b.Property<int>("RebateId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("EnrollmentId");

                    b.Property<decimal>("RebateAmount");

                    b.Property<Guid?>("RebateGuid");

                    b.Property<DateTime>("RebateIssueDate");

                    b.Property<int>("RebateIssueStatus");

                    b.Property<int>("RebateIssued");

                    b.HasKey("RebateId");

                    b.ToTable("Rebate");
                });

            modelBuilder.Entity("UpDiddyApi.Models.ReportEnrollmentByVendor", b =>
                {
                    b.Property<int>("ReportEnrollmentByVendorId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CoursesCompletedCount");

                    b.Property<int>("CoursesDroppedCount");

                    b.Property<int>("EnrollmentsCompleted");

                    b.Property<int>("Month");

                    b.Property<int>("PromoCodesRedeemedCount");

                    b.Property<Guid?>("ReportEnrollmentByVendorGuid");

                    b.Property<decimal>("TotalRefundsIssued");

                    b.Property<decimal>("TotalRevenueIn");

                    b.Property<decimal>("TotalSplitOut");

                    b.Property<int>("VendorId");

                    b.Property<int>("Year");

                    b.HasKey("ReportEnrollmentByVendorId");

                    b.ToTable("ReportEnrollmentByVendors");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Subscriber", b =>
                {
                    b.Property<int>("SubscriberId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Address");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<DateTime?>("DateOfBirth");

                    b.Property<int?>("EducationLevelId");

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("FirstName")
                        .IsRequired();

                    b.Property<int?>("GenderId");

                    b.Property<int>("IsDeleted");

                    b.Property<string>("LastName")
                        .IsRequired();

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("PhoneNumber");

                    b.Property<string>("ProfileImage");

                    b.Property<Guid?>("SubscriberGuid");

                    b.HasKey("SubscriberId");

                    b.ToTable("Subscriber");
                });

            modelBuilder.Entity("UpDiddyApi.Models.SubscriberPromoCode", b =>
                {
                    b.Property<int>("SubscriberPromoCodeId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("PromoCodeId");

                    b.Property<int>("SubscriberId");

                    b.Property<Guid?>("SubscriberPromoCodeGuid");

                    b.HasKey("SubscriberPromoCodeId");

                    b.ToTable("SubscriberPromoCode");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Tag", b =>
                {
                    b.Property<int>("TagId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Guid?>("TagGuid");

                    b.HasKey("TagId");

                    b.ToTable("Tag");
                });

            modelBuilder.Entity("UpDiddyApi.Models.TagCourse", b =>
                {
                    b.Property<int>("TagCourseId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CourseId");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<Guid?>("TagCourseGuid");

                    b.Property<int>("TagId");

                    b.HasKey("TagCourseId");

                    b.ToTable("TagCourse");
                });

            modelBuilder.Entity("UpDiddyApi.Models.TagTopic", b =>
                {
                    b.Property<int>("TagTopicId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<int>("TagId");

                    b.Property<Guid?>("TagTopicGuid");

                    b.Property<int>("TopicId");

                    b.HasKey("TagTopicId");

                    b.ToTable("TagTopic");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Topic", b =>
                {
                    b.Property<int>("TopicId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<string>("Description");

                    b.Property<string>("DesktopImage");

                    b.Property<int>("IsDeleted");

                    b.Property<string>("MobileImage");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Slug")
                        .IsRequired();

                    b.Property<int?>("SortOrder");

                    b.Property<string>("TabletImage");

                    b.Property<Guid?>("TopicGuid");

                    b.HasKey("TopicId");

                    b.ToTable("Topic");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Vendor", b =>
                {
                    b.Property<int>("VendorId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Guid?>("VendorGuid");

                    b.HasKey("VendorId");

                    b.ToTable("Vendor");
                });

            modelBuilder.Entity("UpDiddyApi.Models.VendorPromoCode", b =>
                {
                    b.Property<int>("VendorPromoCodeId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("PromoCodeId");

                    b.Property<int>("VendorId");

                    b.Property<Guid?>("VendorPromoCodeGuid");

                    b.HasKey("VendorPromoCodeId");

                    b.ToTable("VendorPromoCode");
                });

            modelBuilder.Entity("UpDiddyApi.Models.VendorStudentLogin", b =>
                {
                    b.Property<int>("VendorStudentLoginId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<int>("SubscriberId");

                    b.Property<int>("VendorId");

                    b.Property<string>("VendorLogin");

                    b.HasKey("VendorStudentLoginId");

                    b.ToTable("VendorStudentLogin");
                });

            modelBuilder.Entity("UpDiddyApi.Models.VendorTermsOfService", b =>
                {
                    b.Property<int>("VendorTermsOfServiceId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<DateTime>("DateEffective");

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<int>("VendorId");

                    b.Property<Guid?>("VendorTermsOfServiceGuid");

                    b.HasKey("VendorTermsOfServiceId");

                    b.ToTable("VendorTermsOfService");
                });

            modelBuilder.Entity("UpDiddyApi.Models.WozCourseEnrollment", b =>
                {
                    b.Property<int>("WozCourseEnrollmentId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<long>("EnrollmentDateUTC");

                    b.Property<int>("EnrollmentId");

                    b.Property<int>("EnrollmentStatus");

                    b.Property<int>("ExeterId");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<int>("SectionId");

                    b.Property<int>("WozEnrollmentId");

                    b.HasKey("WozCourseEnrollmentId");

                    b.ToTable("WozCourseEnrollment");
                });

            modelBuilder.Entity("UpDiddyApi.Models.WozCourseSection", b =>
                {
                    b.Property<int>("WozCourseSectionId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CourseCode");

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<int>("Month");

                    b.Property<int>("Section");

                    b.Property<int>("Year");

                    b.HasKey("WozCourseSectionId");

                    b.ToTable("WozCourseSection");
                });

            modelBuilder.Entity("UpDiddyApi.Models.WozTermsOfService", b =>
                {
                    b.Property<int>("WozTermsOfServiceId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<int>("DocumentId");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("TermsOfService");

                    b.HasKey("WozTermsOfServiceId");

                    b.ToTable("WozTermsOfService");
                });

            modelBuilder.Entity("UpDiddyApi.Models.WozTransactionLog", b =>
                {
                    b.Property<int>("WozTransactionLogId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate");

                    b.Property<Guid>("CreateGuid");

                    b.Property<string>("EndPoint");

                    b.Property<Guid?>("EnrollmentGuid");

                    b.Property<string>("InputParameters");

                    b.Property<int>("IsDeleted");

                    b.Property<DateTime?>("ModifyDate");

                    b.Property<Guid?>("ModifyGuid");

                    b.Property<string>("ResponseJson");

                    b.Property<string>("WozResponseJson");

                    b.HasKey("WozTransactionLogId");

                    b.ToTable("WozTransactionLog");
                });

            modelBuilder.Entity("UpDiddyApi.Models.Enrollment", b =>
                {
                    b.HasOne("UpDiddyApi.Models.Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("UpDiddyApi.Models.Subscriber", "Subscriber")
                        .WithMany()
                        .HasForeignKey("SubscriberId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
