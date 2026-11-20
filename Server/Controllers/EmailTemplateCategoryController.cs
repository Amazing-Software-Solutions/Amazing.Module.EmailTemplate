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
    public class EmailTemplateCategoryController : ModuleControllerBase
    {
        private readonly IEmailTemplateCategoryService _EmailTemplateCategoryService;

        public EmailTemplateCategoryController(IEmailTemplateCategoryService EmailTemplateCategoryService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _EmailTemplateCategoryService = EmailTemplateCategoryService;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.EmailTemplateCategory>> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _EmailTemplateCategoryService.GetEmailTemplateCategoriesAsync(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplateCategory Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5/1
        [HttpGet("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.EmailTemplateCategory> Get(int id, int moduleid)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                Models.EmailTemplateCategory EmailTemplateCategory = await _EmailTemplateCategoryService.GetEmailTemplateCategoryAsync(id, moduleid);
                if (EmailTemplateCategory != null)
                {
                    return EmailTemplateCategory;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "EmailTemplateCategory Not Found Or Unauthorized {EmailTemplateCategoryId} {ModuleId}", id, moduleid);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplateCategory Get Attempt {EmailTemplateCategoryId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.EmailTemplateCategory> Post([FromBody] Models.EmailTemplateCategory EmailTemplateCategory, [FromQuery] int moduleid)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                EmailTemplateCategory = await _EmailTemplateCategoryService.AddEmailTemplateCategoryAsync(EmailTemplateCategory, moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplateCategory Post Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                EmailTemplateCategory = null;
            }
            return EmailTemplateCategory;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.EmailTemplateCategory> Put(int id, [FromBody] Models.EmailTemplateCategory EmailTemplateCategory, [FromQuery] int moduleid)
        {
            if (ModelState.IsValid && EmailTemplateCategory.EmailTemplateCategoryId == id && IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                EmailTemplateCategory = await _EmailTemplateCategoryService.UpdateEmailTemplateCategoryAsync(EmailTemplateCategory, moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplateCategory Put Attempt {EmailTemplateCategoryId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                EmailTemplateCategory = null;
            }
            return EmailTemplateCategory;
        }

        // DELETE api/<controller>/5/1
        [HttpDelete("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleid)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                await _EmailTemplateCategoryService.DeleteEmailTemplateCategoryAsync(id, moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EmailTemplateCategory Delete Attempt {EmailTemplateCategoryId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
