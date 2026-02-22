using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Amazing.Module.EmailTemplate.Services;
using Oqtane.Controllers;
using System.Net;
using System.Threading.Tasks;

namespace Amazing.Module.EmailTemplate.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class EmailTemplateController : ModuleControllerBase
    {
        private readonly IEmailTemplateService _EmailTemplateService;

        public EmailTemplateController(IEmailTemplateService EmailTemplateService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _EmailTemplateService = EmailTemplateService;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.EmailTemplate>> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _EmailTemplateService.GetEmailTemplatesAsync(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplate Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.EmailTemplate> Get(int id, int moduleid)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                Models.EmailTemplate EmailTemplate = await _EmailTemplateService.GetEmailTemplateAsync(id, moduleid);
                if (EmailTemplate != null)
                {
                    return EmailTemplate;
                }
                else
                { 
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "EmailTemplate Not Found Or Unauthorized {EmailTemplateId} {ModuleId}", id, moduleid);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplate Get Attempt {EmailTemplateId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.EmailTemplate> Post([FromBody] Models.EmailTemplate EmailTemplate, [FromQuery] int moduleid)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                EmailTemplate = await _EmailTemplateService.AddEmailTemplateAsync(EmailTemplate, moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplate Post Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                EmailTemplate = null;
            }
            return EmailTemplate;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.EmailTemplate> Put(int id, [FromBody] Models.EmailTemplate EmailTemplate, [FromQuery] int moduleid)
        {
            if (ModelState.IsValid && EmailTemplate.EmailTemplateId == id && IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                EmailTemplate = await _EmailTemplateService.UpdateEmailTemplateAsync(EmailTemplate, moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplate Put Attempt {EmailTemplateId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                EmailTemplate = null;
            }
            return EmailTemplate;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleid)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                await _EmailTemplateService.DeleteEmailTemplateAsync(id, moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplate Delete Attempt {EmailTemplateId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // POST api/<controller>/sendtest/5
        [HttpPost("sendtest/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.TestEmailResult> SendTest(int moduleid, [FromBody] Models.TestEmailRequest request)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                return await _EmailTemplateService.SendTestEmailAsync(
                    moduleid,
                    request.ToEmail,
                    request.Subject,
                    request.Body,
                    request.VariableJson
                );
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Test Email Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return new Models.TestEmailResult { Success = false, Message = "Unauthorized" };
            }
        }
    }
}
