using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Demo.ScatterGather.Core.Events;
using MassTransit;

namespace Demo.ScatterGather.Claims.CptValidator
{
    public class ClaimValidationRequestedConsumer : IConsumer<ClaimValidationRequested>
    {
        private static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri("https://ics.nthrive.com/ICSServices/"),
            DefaultRequestHeaders =
            {
                {"SOAPAction", "http://medassets.com/ics/ICPTService/GetCPTAdvanced"},
                {"X-MedAssets-ICS-AuthToken", ConfigurationManager.AppSettings["IcsAuthToken"]}
            }
        };

        private const string SoapRequestFormat =
            @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ics=""http://medassets.com/ics"" xmlns:med=""http://schemas.datacontract.org/2004/07/MedAssets.ICS.Common.Objects.CPTAdvanced"">
        <soapenv:Header/>
        <soapenv:Body>
            <ics:GetCPTAdvanced>
                <!--Optional:-->
                <ics:request>
                    <!--Optional:-->
                    <med:Lookups>
                        <!--Zero or more repetitions:-->
                        <med:CPTAdvancedLookup>
                        <!--Optional:-->
                        <med:CPTCode>{0}</med:CPTCode>
                        </med:CPTAdvancedLookup>
                    </med:Lookups>
                </ics:request>
            </ics:GetCPTAdvanced>
        </soapenv:Body>
        </soapenv:Envelope>";

        public async Task Consume(ConsumeContext<ClaimValidationRequested> context)
        {
            var errors = new List<string>();
            var services = context.Message.Claim.Services;

            foreach (var service in services)
            {
                if (service.CodeType == CodeType.Cpt)
                {
                    var soapRequest = string.Format(SoapRequestFormat, service.Code);
                    var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

                    var response = await Client.PostAsync("CPTService.svc", content);
                    var soapResponse = await response.Content.ReadAsStringAsync();

                    var soap = XDocument.Parse(soapResponse);
                    XNamespace ns = "http://medassets.com/ics";
                    XNamespace nsa = "http://schemas.datacontract.org/2004/07/MedAssets.ICS.Common.Objects.CPTAdvanced";
                    XNamespace nsb = "http://schemas.datacontract.org/2004/07/Medassets.ICS.Common.Objects";

                    var cptErrors = soap.Descendants(ns + "GetCPTAdvancedResponse").First()
                        .Element(ns + "GetCPTAdvancedResult")
                        .Element(nsa + "LookupResults")
                        .Descendants(nsa + "CPTAdvancedLookupResult").First()
                        .Element(nsa + "Errors");

                    if (cptErrors != null)
                    {
                        errors.Add(cptErrors.Descendants(nsb + "ICSError").First()?
                            .Element(nsb + "Message")?.Value);
                    }
                }
            }

            context.Respond(new ClaimValidated
            {
                ValidatorName = typeof(ClaimValidationRequestedConsumer).Namespace,
                Errors = errors.ToArray()
            });
        }
    }
}