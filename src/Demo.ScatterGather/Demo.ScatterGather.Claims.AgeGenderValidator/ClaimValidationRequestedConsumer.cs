using System;
using System.Collections.Generic;
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
                {"X-MedAssets-ICS-AuthToken", "123456789ABCDE"}
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
            var errors = new List<string>();
            var patient = context.Message.Claim.Patient;
            var services = context.Message.Claim.Services;
            const string dateFormat = "yyy-MM-dd";

            foreach (var service in services)
            {
                if (service.CodeType == CodeType.Cpt)
                {
                    var soapRequest = string.Format(SoapRequestFormat, service.Code, patient.Birthdate.ToString(dateFormat),
                        service.ServiceDate.ToString(dateFormat), patient.Gender, service.CodeType.ToString().ToUpper());
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
                        errors.Add(ageResult?.Element(nsa + "AgeRuleMessage")?.Value);
                    }

                    var genderResult = result.Element(nsa + "GenderEdit");
                    var genderIsValid = genderResult?.Element(nsa + "ValidType")?.Value == "OK";

                    if (genderIsValid == false)
                    {
                        errors.Add(genderResult?.Element(nsa + "GenderRuleMessage")?.Value);
                    }
                }
            }

            /*
            if (context.Message.Per.BirthDate > new DateTime(1980, 1, 1))
            {
                errors.Add($"Birthdate of {context.Message.Person.BirthDate.ToShortDateString()} should be greater than 1/1/1980.");
            }
            */

            context.Respond(new ClaimValidated
            {
                ValidatorName = typeof(ClaimValidationRequestedConsumer).Namespace,
                Errors = errors.ToArray()
            });
        }
    }
}