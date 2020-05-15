using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Services.B2B.CareerTalentPipeline;
using UpDiddyLib.Domain.Models.B2B;
using UpDiddyLib.Helpers;
using Xunit;

namespace API.Tests.B2B
{
    public class CreateTalentPipelineTests
    {
        [Fact]
        public void CanGetQuestionList()
        {
            // Arrange
            var question1 = "Question1";
            var question2 = "Question2";

            var mockEmailService = new Mock<ISysEmail>();
            var options = new CareerTalentPipelineOptions
            {
                Questions = { question1, question2 }
            };

            var careerTalentPipelineService = new CareerTalentPipelineService(
                mockEmailService.Object,
                Options.Create(options));

            // Act
            var questions = careerTalentPipelineService.GetQuestions();

            // Assert
            Assert.NotNull(questions);
            Assert.Equal(2, questions.Count);
            Assert.Equal(question1, questions[0]);
            Assert.Equal(question2, questions[1]);
        }

        [Fact]
        public async Task CanSubmitQandA()
        {
            // Arrange
            var responseEmailAddress = "email1@email.com";
            var responseEmailTemplateId = "abc123";
            var responseEmailSubject = "Thank you";

            var ccEmailAddress = "email2@email.com";
            var ccEmailTemplateId = "def456";
            var ccEmailSubject = "Custom Talent Pipeline";
            var phoneNumber = "123456789";
            var preferences = ContactPreferences.Text;

            var question1 = "Question1";
            var question2 = "Question2";
            var answer1 = "Answer1";
            var answer2 = "Answer2";

            var options = new CareerTalentPipelineOptions
            {
                ccEmail = ccEmailAddress,
                ccEmailTemplateId = ccEmailTemplateId,
                ResponseEmailTemplateId = responseEmailTemplateId,
                Questions = { question1, question2 }
            };

            var dto = new CareerTalentPipelineDto
            {
                PhoneNumber = phoneNumber,
                Preferences = preferences,
                Email = responseEmailAddress,
                Questions = {
                    { question1, answer1 },
                    { question2, answer2 }
                }
            };

            var mockEmailService = new Mock<ISysEmail>();
            mockEmailService
                .Setup(service => service.SendTemplatedEmailAsync(
                    responseEmailAddress,
                    responseEmailTemplateId,
                    null,
                    Constants.SendGridAccount.Transactional,
                    responseEmailSubject,
                    null,
                    null,
                    null))
                .ReturnsAsync(true)
                .Verifiable();

            mockEmailService
                .Setup(service => service.SendTemplatedEmailAsync(
                    ccEmailAddress,
                    ccEmailTemplateId,
                    It.IsAny<object>(),
                    Constants.SendGridAccount.Transactional,
                    ccEmailSubject,
                    null,
                    null,
                    null))
                .ReturnsAsync(true)
                .Verifiable();

            var careerTalentPipelineService = new CareerTalentPipelineService(
                mockEmailService.Object,
                Options.Create(options));

            // Act
            var successful = await careerTalentPipelineService.SubmitCareerTalentPipeline(dto);

            // Assert
            Assert.True(successful);

            mockEmailService.VerifyAll();
        }
    }
}
