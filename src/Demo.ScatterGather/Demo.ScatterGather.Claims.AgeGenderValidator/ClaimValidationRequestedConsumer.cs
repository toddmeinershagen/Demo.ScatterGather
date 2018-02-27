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

namespace Demo.ScatterGather.Claims.AgeGenderValidator
{
    public class ClaimValidationRequestedConsumer : IConsumer<ClaimValidationRequested>
    {
        private static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri("https://ics.nthrive.com/ICSServices/"),
            DefaultRequestHeaders =
            {
                {"SOAPAction", "http://medassets.com/ics/IMedNecService/GetAgeGenderRule"},
                {"X-MedAssets-ICS-AuthToken", ConfigurationManager.AppSettings["IcsAuthToken"]}
            }
        };

        private const string SoapRequestFormat =
            @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ics=""http://medassets.com/ics"" xmlns:med=""http://schemas.datacontract.org/2004/07/MedAssets.ICS.WebService.MedNec.ExportDefinition"" xmlns:arr=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">
        <soapenv:Header/>
        <soapenv:Body>
            <ics:GetAgeGenderRule>
                <!--Optional:-->
                <ics:request>
                    <!--Optional:-->
                    <med:AgeGenderRuleLookups>
                        <!--Zero or more repetitions:-->
                        <med:AgeGenderRuleLookup>
                            <!--Optional:-->
                            <med:Identifier>{0}</med:Identifier>
                            <!--Optional:-->
                            <med:BirthDate>{1}</med:BirthDate>
                            <!--Optional:-->
                            <med:ServiceDate>{2}</med:ServiceDate>
                            <!--Optional:-->
                            <med:Gender>{3}</med:Gender>
                            <!--Optional:-->
                            <med:CodeType>{4}</med:CodeType>
                        </med:AgeGenderRuleLookup>
                    </med:AgeGenderRuleLookups>
                    <!--Optional:-->
                    <med:Parameters>
                        <!--Zero or more repetitions:-->
                        <arr:KeyValueOfstringstring>
                            <arr:Key></arr:Key>
                            <arr:Value></arr:Value>
                        </arr:KeyValueOfstringstring>
                    </med:Parameters>
                </ics:request>
                <!--Optional:-->
                <ics:isExcludeAgeEdit>0</ics:isExcludeAgeEdit>
                <!--Optional:-->
                <ics:isExcludeGenderEdit>0</ics:isExcludeGenderEdit>
            </ics:GetAgeGenderRule>
        </soapenv:Body>
        </soapenv:Envelope>";

        public async Task Consume(ConsumeContext<ClaimValidationRequested> context)
        {
            await Console.Out.WriteLineAsync($"Request received.for {typeof(ClaimValidationRequestedConsumer).FullName}.");

            var errors = new List<ClaimValidationError>();
            var patient = context.Message.Claim.Patient;
            var services = context.Message.Claim.Services;
            const string dateFormat = "yyy-MM-dd";

            foreach (var service in services)
            {
                if (service.CodeType == CodeType.Cpt)
                {
                    var code = service.Code;
                    var birthdate = patient.Birthdate.ToString(dateFormat);
                    var serviceDate = service.ServiceDate.ToString(dateFormat);
                    var codeType = service.CodeType.ToString().ToUpper();

                    var soapRequest = string.Format(SoapRequestFormat, code, birthdate,
                        serviceDate, patient.Gender, codeType);
                    var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

                    var response = await Client.PostAsync("MedNecService.svc", content);
                    var soapResponse = await response.Content.ReadAsStringAsync();

                    var soap = XDocument.Parse(soapResponse);
                    XNamespace ns = "http://medassets.com/ics";
                    XNamespace nsa = "http://schemas.datacontract.org/2004/07/MedAssets.ICS.WebService.MedNec.ExportDefinition";

                    var result = soap.Descendants(ns + "GetAgeGenderRuleResponse").First()
                        .Element(ns + "GetAgeGenderRuleResult")
                        .Element(nsa + "AgeGenderRule");

                    var ageResult = result.Element(nsa + "AgeEdit");
                    var ageIsValid = ageResult?.Element(nsa + "ValidType")?.Value == "OK";

                    if (ageIsValid == false)
                    {
                        var message = ageResult?.Element(nsa + "AgeRuleMessage")?.Value;
                        errors.Add(new ClaimValidationError {Value = $"{{'Birthdate':'{birthdate}', 'ServiceDate':'{serviceDate}', 'Code':'{code}', 'CodeType':'{codeType}'}}", Message = message});
                    }

                    var genderResult = result.Element(nsa + "GenderEdit");
                    var genderIsValid = genderResult?.Element(nsa + "ValidType")?.Value == "OK";

                    if (genderIsValid == false)
                    {
                        var message = genderResult?.Element(nsa + "GenderRuleMessage")?.Value;
                        errors.Add(new ClaimValidationError {Value = $"{{'Gender':'{patient.Gender}'}}", Message = message});
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