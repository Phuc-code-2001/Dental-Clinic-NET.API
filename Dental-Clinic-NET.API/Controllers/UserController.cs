﻿using AutoMapper;
using DataLayer.Domain;
using Dental_Clinic_NET.API.DTO;
using Dental_Clinic_NET.API.Models.Users;
using Dental_Clinic_NET.API.Permissions;
using Dental_Clinic_NET.API.Serializers;
using ImageProcessLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dental_Clinic_NET.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IMapper _mapper;
        private UserManager<BaseUser> _userManager;
        private ImageKitServices _imageKitServices;

        public UserController(UserManager<BaseUser> userManager, ImageKitServices imageKitServices, IMapper mapper)
        {
            _userManager = userManager;
            _imageKitServices = imageKitServices;
            _mapper = mapper;
        }

        /// <summary>
        ///     Create account with role=Administrator when enable application
        /// </summary>
        /// <param name="inputInfo">Account Info</param>
        /// <returns>
        ///     400: Superuser already exist || Info invalid
        ///     200: Create success
        ///     500: Server Handle Error
        ///     
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> CreateSuperUserAsync(CreateSuperUserModel inputInfo)
        {

            try
            {

                if (_userManager.Users.Any(user => user.Type == UserType.Administrator))
                {
                    return BadRequest("Superuser already exist...");
                }

                BaseUser user = inputInfo.ToBaseUser_NotIncludePassword();
                user.Type = UserType.Administrator;
                IdentityResult result = await _userManager.CreateAsync(user, inputInfo.Password);

                if (result.Succeeded)
                {
                    return Ok("Ok...");
                }

                var errors = result.Errors.Select(er => new { er.Code, er.Description });

                return BadRequest(new
                {
                    errorCount = errors.Count(),
                    errors = errors,
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        /// <summary>
        /// Get User information by JWT after login
        /// </summary>
        /// <returns>
        ///     UserDTO: User information
        /// </returns>
        [HttpGet]
        [Authorize]
        [EnableQuery]
        public async Task<IActionResult> GetAuthorizeAsync()
        {
            try
            {
                BaseUser loggedUser = await _userManager.FindByIdAsync(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "");
                UserSerializer serializer = new UserSerializer(new PermissionOnBaseUser(loggedUser, loggedUser));

                return Ok(serializer.Serialize(user =>
                {
                    return _mapper.Map<UserDTO>(user);
                }));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
        }

        /// <summary>
        /// Update avatar for User
        /// </summary>
        /// <param name="userId">Id of User</param>
        /// <param name="image">Image file</param>
        /// <returns>
        ///     401: not is_admin, not is_owner, not is_authenticated
        ///     400: Image file invalid
        ///     500: ImageKit server error || Server handle error
        ///     200: { string: newImage, UserDTO: user }
        /// </returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateAvatarAsync(string userId, IFormFile image)
        {
            try
            {
                BaseUser requiredUser = await _userManager.FindByIdAsync(userId);
                BaseUser loginUser = await _userManager.FindByNameAsync(User.Identity.Name);

                PermissionOnBaseUser permission = new PermissionOnBaseUser(loginUser, requiredUser);

                if(!permission.IsAdmin && !permission.IsOwner)
                {
                    return Unauthorized("Cann't do this operation");
                }

                if (_imageKitServices.IsImage(image))
                {
                    var result = await _imageKitServices.UploadImageAsync(image, image.FileName);
                    if(requiredUser.ImageAvatarId != null)
                    {
                        await _imageKitServices.DeleteImageAsync(requiredUser.ImageAvatarId);
                    }
                    requiredUser.ImageURL = result.URL;
                    requiredUser.ImageAvatarId = result.ImageId;
                    await _userManager.UpdateAsync(requiredUser);

                    UserSerializer serializer = new UserSerializer(permission);
                    return Ok(new
                    {
                        newImage=requiredUser.ImageURL,
                        user=serializer.Serialize(user => _mapper.Map<UserDTO>(user))
                    });
                }

                return BadRequest("File must be image");
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        ///     Require administrator role. List all users within ODATA, each page include max 10 users
        /// </summary>
        /// <returns>
        ///     200: Query success
        ///     500: Server handle error
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [EnableQuery(PageSize = 10)]
        public IActionResult GetUsers()
        {
            try
            {
                var users = _userManager.Users.ToList();
                var usersDTO = users.Select(user => _mapper.Map<UserDTO>(user)).ToList();

                return Ok(usersDTO.AsQueryable());

            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
