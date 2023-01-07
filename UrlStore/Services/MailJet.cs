using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json.Linq;

namespace UrlStore.Services
{
    public class MailJet : IEmailSender
    {
        private readonly IConfiguration configuration;
        private MailJetOptions mailJetOptions;

        public MailJet(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            mailJetOptions = configuration.GetSection("MailJet")
                .Get<MailJetOptions>();

            MailjetClient client = new MailjetClient(mailJetOptions.ApiKey, mailJetOptions.SecretKey)
            {
                Version = ApiVersion.V3_1,
            };

            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(Send.Messages, new JArray {
                new JObject {
                {
                    "From",
                    new JObject {
                        {"Email", "leomarqz2020@gmail.com"},
                        {"Name", "leomarqz"}
                    }
                },
                {
                    "To",
                    new JArray {
                        new JObject {
                            {
                                "Email",
                                email
                            }, 
                            {
                                "Name",
                                email
                            }
                        }
                    }
                },
                {
                    "Subject",
                    subject
                },
                {
                    "HTMLPart",
                    htmlMessage
                }
             }
             });
            await client.PostAsync(request);
        }
    }
}
