using System.Net;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using G2Interfaces = UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyApi.Authorization;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Dto.User;
using Microsoft.AspNetCore.Authorization;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.AzureSearch;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Domain.Models.G2;
using System.Collections.Generic;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using AutoMapper;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Exceptions;
using static UpDiddyLib.Helpers.Constants;
using UpDiddyLib.Helpers;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace UpDiddyApi.ApplicationCore.Services
{


    public class SendGridService : ISendGridService
    {
        private readonly IConfiguration _configuration;
        private readonly IG2Service _g2Service;
        private readonly IHangfireService _hangfireService;
        private readonly G2Interfaces.IProfileService _profileService;
        private readonly G2Interfaces.IWishlistService _wishlistService;
        private readonly G2Interfaces.ICommentService _commentService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ITagService _tagService;
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger _syslog;
        private readonly ISysEmail _sysEmail;
 

        public SendGridService(IServiceProvider services, IRepositoryWrapper repositoryWrapper, IMapper mapper, IHangfireService hangfireService, IConfiguration configuration, ILogger<SendGridService> logger, ISysEmail sysEmail)
        {

            _configuration = services.GetService<IConfiguration>();
            _g2Service = services.GetService<IG2Service>();
            _hangfireService = services.GetService<IHangfireService>();
            _profileService = services.GetService<G2Interfaces.IProfileService>();
            _wishlistService = services.GetService<G2Interfaces.IWishlistService>();
            _commentService = services.GetService<G2Interfaces.ICommentService>();
            _emailTemplateService = services.GetService<IEmailTemplateService>();
            _tagService = services.GetService<ITagService>();
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _syslog = logger;
            _sysEmail = sysEmail;
        }


        public async Task<bool> SendBulkEmailByList(Guid TemplateGuid, List<Guid> Profiles )
        {            

            // validate profiles have been specified 
            if (Profiles == null || Profiles.Count == 0)
                throw new FailedValidationException($"SendGridService:SendBulkEmailsByList  One or more profiles must be specified for bulk email");

            // validate the email template 
            EmailTemplate template = await _repositoryWrapper.EmailTemplateRepository.GetByGuid(TemplateGuid);

            if ( template == null )
                throw new FailedValidationException($"SendGridService:SendBulkEmailsByList {TemplateGuid} is not a valid bulk email template");

            // validate the api key 
            if ( ValidateSendgridSubAccount(template.SendGridSubAccount) == false )
                throw new FailedValidationException($"SendGridService:SendBulkEmailsByList {template.SendGridSubAccount} is not a valid SendGrid subaccount");

            // get the list of email associated with the profiles 
            List<UpDiddyApi.Models.G2.Profile> profiles = await _profileService.GetProfilesByGuidList(Profiles);


            foreach (Models.G2.Profile p in profiles)
            {
                dynamic templateData = null;
                try
                {
                    templateData = BuildEmailTemplate(p, template.TemplateParams);
              
                }
                catch ( Exception ex )
                {
                    _syslog.LogError($"SendGridService:SendbulkEmailByList  Error generating email data for {p.ProfileGuid} Error = {ex.Message} ");

                }

                // TODO JAB Try catch and log this 
                SendGridAccount accountType = (SendGridAccount) Enum.Parse(typeof(SendGridAccount), template.SendGridSubAccount);
                // todo jab try catch and log on buildEmailTempate 
                _sysEmail.SendTemplatedEmailAsync(p.Email, template.SendGridTemplateId, templateData, accountType );
            }



  
            





            // todo jab loop through each profile guid, find subscriber and call send email 

            return true;

        }

        public async Task<EmailTemplateListDto> GetEmailTemplates(int limit, int offset, string sort, string order)
        {
            // todo jab implement 
            EmailTemplateListDto rval = await _emailTemplateService.GetEmailTemplates(limit,offset,sort,order);

            return rval;
        }


        #region Private Helpers




        /*

            TODO JAB Remove but Save very cool 


        AssemblyBuilder dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("MyDynamicAssembly"),
   AssemblyBuilderAccess.Run);
        ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("MyDynamicAssemblyModule");
        TypeBuilder dynamicType = dynamicModule.DefineType("MyDynamicType", TypeAttributes.Public);
        AddProperty(dynamicType, "firstName", typeof(string));
        AddProperty(dynamicType, "description", typeof(string));
        AddProperty(dynamicType, "courseUrl", typeof(string));
        Type myType = dynamicType.CreateType();
        var instantiatedObject = Activator.CreateInstance(myType);


        PropertyInfo prop1 = myType.GetProperty("firstName");
        prop1.SetValue(instantiatedObject, "This is the first name ", null);
            PropertyInfo prop2 = myType.GetProperty("description");
        prop2.SetValue(instantiatedObject, "This is the description", null);
            PropertyInfo prop3 = myType.GetProperty("courseUrl");
        prop3.SetValue(instantiatedObject, "This is the courseUrl", null);

        private    void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig;

            FieldBuilder field = typeBuilder.DefineField("_" + propertyName, typeof(string), FieldAttributes.Private);
            PropertyBuilder property = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType,
                new[] { propertyType });

            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_value", getSetAttr, propertyType,
                Type.EmptyTypes);
            ILGenerator getIl = getMethodBuilder.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, field);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_value", getSetAttr, null,
                new[] { propertyType });
            ILGenerator setIl = setMethodBuilder.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, field);
            setIl.Emit(OpCodes.Ret);

            property.SetGetMethod(getMethodBuilder);
            property.SetSetMethod(setMethodBuilder);
        }

    */


        private dynamic BuildEmailTemplate(Models.G2.Profile p, string TemplateParams)
        {
  
            string[] props = TemplateParams.Split(';');

            JObject rval = new JObject();
           
            foreach ( string prop in props)
            {
                if ( string.IsNullOrEmpty(prop) == false )
                {
                    string [] info = prop.Split(':');
                    string path = info[0];
                    string paramName = info[1];
                    var val = GetPropertyValue(p, path);
                    if ( val != null )
                    {
                        JToken paramVal = JToken.FromObject(val);
                        rval.Add(paramName, paramVal);
                    }                    
                }
            }
 
            return rval;
        }


        private  object GetPropertyValue(object src, string propName)
        {
            if (src == null) throw new ArgumentException("Value cannot be null.", "src");
            if (propName == null) throw new ArgumentException("Value cannot be null.", "propName");

            if (propName.Contains("."))//complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);

                var prop = src.GetType().GetProperty(temp[0]);
                if (prop == null)
                    return null;

                var subObject = prop.GetValue(src, null);
                if (subObject == null)
                    return null;

                return GetPropertyValue(GetPropertyValue(src, temp[0]), temp[1]);
            }
            else
            {
                var prop = src.GetType().GetProperty(propName);
                return prop != null ? prop.GetValue(src, null) : null;
            }
        }



        private bool ValidateSendgridSubAccount(string accountName)
        {

            string apiKey =  _configuration[$"SysEmail:{accountName}:ApiKey"];
            return apiKey != null;

        }

        #endregion


    }
}
