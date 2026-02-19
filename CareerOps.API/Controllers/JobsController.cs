using CareerOps.Application.Common;
using CareerOps.Application.DTOs;
using CareerOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CareerOps.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost("{id}/analyze")]
    public async Task<IActionResult> Analyze([FromRoute] Guid id)
    {
        var result = await _jobService.AnalyzeJobAsync(id);

        return ProcessResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(JobApplicationRequest request)
    {
        var result = await _jobService.CreateJobAsync(request);

        return ProcessCreatedResult(result, nameof(GetById));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _jobService.GetJobByIdAsync(id);
        return ProcessResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _jobService.GetAllJobsAsync();

        return ProcessResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateJobRequest request)
    {
        var result = await _jobService.UpdateJobAsync(id, request);

        return ProcessResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _jobService.DeleteJobAsync(id);

        return ProcessResult(result);
    }

    [NonAction]
    private IActionResult ProcessCreatedResult<T>(Result<T> result, string actionName)
    {
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        

        return CreatedAtAction(actionName, new { id = result.Value }, result.Value);
    }

    [NonAction]
    private IActionResult ProcessResult<T>(Result<T> result)
    {
        if (result.IsFailure)
        {
            return result.Error != null && result.Error.Contains("não encontrado")
                ? NotFound(new { message = result.Error })
                : BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [NonAction]
    private IActionResult ProcessResult(Result result)
    {
        if (result.IsFailure)
        {
            return result.Error != null && result.Error.Contains("não encontrado")
                ? NotFound(new { message = result.Error })
                : BadRequest(new { message = result.Error });
        }

        return NoContent(); 
    }
}