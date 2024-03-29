﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Works.BlogProject.Business.Interfaces;
using Works.BlogProject.Dto.DTOs.BlogDtos;
using Works.BlogProject.Dto.DTOs.CategoryBlogDtos;
using Works.BlogProject.Dto.DTOs.CategoryDtos;
using Works.BlogProject.Dto.DTOs.CommentDtos;
using Works.BlogProject.Entities.Concrete;
using Works.BlogProject.WebApi.CustomFilters;
using Works.BlogProject.WebApi.Enums;
using Works.BlogProject.WebApi.Models;

namespace Works.BlogProject.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogsController : BaseController
    {
        private readonly ICommentService _commentService;
        private readonly IBlogService _blogService;
        private readonly IMapper _mapper;
        public BlogsController(IBlogService blogService, IMapper mapper, ICommentService commentService)
        {
            _commentService = commentService;
            _mapper = mapper;
            _blogService = blogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(_mapper.Map<List<BlogListDto>>(await _blogService.GetAllSortedPostedTimeAsync()));
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(ValidId<Blog>))]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(_mapper.Map<BlogListDto>(await _blogService.GetByIdAsync(id)));
        }

        [HttpPost]
        [Authorize]
        [ValidModel]
        public async Task<IActionResult> Create([FromForm] BlogAddModel blogAddModel)
        {

            var uploadModel = await UploadFormFileAsync(blogAddModel.FormFile, "image/jpeg", "blog");

            if (uploadModel.UploadState == UploadState.Success)
            {
                blogAddModel.ImagePath = uploadModel.ImageName;
                await _blogService.InsertAsync(_mapper.Map<Blog>(blogAddModel));
                return Created("", blogAddModel);
            }
            else if (uploadModel.UploadState == UploadState.NotExists)
            {
                await _blogService.InsertAsync(_mapper.Map<Blog>(blogAddModel));
                return Created("", blogAddModel);
            }
            else return BadRequest(uploadModel.ErrorMessage);

        }

        [HttpPut("{id}")]
        [Authorize]
        [ServiceFilter(typeof(ValidId<Blog>))]
        [ValidModel]
        public async Task<IActionResult> Update(int id, [FromForm] BlogUpdateModel blogUpdateModel)
        {
            if (id != blogUpdateModel.Id)
                return BadRequest();

            var uploadModel = await UploadFormFileAsync(blogUpdateModel.FormFile, "image/jpeg", "blog");

            if (uploadModel.UploadState == UploadState.Success)
            {
                blogUpdateModel.ImagePath = uploadModel.ImageName;
                await _blogService.UpdateAsync(_mapper.Map<Blog>(blogUpdateModel));
                return NoContent();
            }
            else if (uploadModel.UploadState == UploadState.NotExists)
            {
                var updatedBlog = await _blogService.GetByIdAsync(blogUpdateModel.Id);
                blogUpdateModel.ImagePath = updatedBlog.ImagePath;
                await _blogService.UpdateAsync(_mapper.Map<Blog>(blogUpdateModel));
                return NoContent();
            }
            else return BadRequest(uploadModel.ErrorMessage);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ServiceFilter(typeof(ValidId<Blog>))]
        public async Task<IActionResult> Delete(int id)
        {
            await _blogService.DeleteAsync(await _blogService.GetByIdAsync(id));
            return NoContent();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddToCategory(CategoryBlogDto categoryBlogDto)
        {
            await _blogService.AddToCategoryAsync(categoryBlogDto);
            return Created("", categoryBlogDto);
        }

        [HttpDelete("[action]")]
        [ValidModel]
        public async Task<IActionResult> RemoveFromCategory(CategoryBlogDto categoryBlogDto)
        {
            await _blogService.RemoveFromCategoryAsync(categoryBlogDto);
            return NoContent();
        }

        [HttpGet("[action]/{id}")]
        [ServiceFilter(typeof(ValidId<Category>))]
        public async Task<IActionResult> GetAllByCategoryId(int id)
        {
            return Ok(await _blogService.GetAllByCategoryIdAsync(id));
        }

        [ServiceFilter(typeof(ValidId<Blog>))]
        [HttpGet("{id}/[action]")]
        public async Task<IActionResult> GetCategories(int id)
        {
            return Ok(_mapper.Map<List<CategoryListDto>>(await _blogService.GetCategoriesWithBlogAsync(id)));
        }

        [HttpGet("{id}/[action]")]
        public async Task<IActionResult> GetComments([FromRoute] int id, [FromQuery] int? parentCommentId)
        {
            return Ok(_mapper.Map<List<CommentListDto>>(await _commentService.GetAllWithSubComments(id, parentCommentId)));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Search([FromQuery] string s)
        {
            return Ok(_mapper.Map<List<BlogListDto>>(await _blogService.SearchAsync(s)));
        }

        [HttpPost("[action]")]
        [ValidModel]
        public async Task<IActionResult> AddComment(CommentAddDto commentAddDto)
        {
            commentAddDto.PostedTime = DateTime.Now;
            await _commentService.InsertAsync(_mapper.Map<Comment>(commentAddDto));
            return Created("", commentAddDto);
        }
    }


}
