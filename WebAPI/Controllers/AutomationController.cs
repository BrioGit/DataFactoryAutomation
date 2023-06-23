using DataFactoryAutomation;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Data.Database;
using WebAPI.Data.Services;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AutomationController : ControllerBase
{
    private readonly Business.DataFactoryAutomation _adfAutomation;
    private readonly IDataFactoryService _adfService;
    private readonly ConfigService _configService;

    public AutomationController(IDataFactoryService adfService, ConfigService configService)
    {
        _adfService = adfService;
        _configService = configService;
        _adfAutomation = new Business.DataFactoryAutomation(_adfService, _configService);
    }

    /// <summary>
    ///     Gets all available Data Factories that can be created or executed in Azure
    /// </summary>
    /// <returns>
    ///     List of data factories
    /// </returns>
    [ProducesResponseType(typeof(List<DataFactory>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    [HttpGet]
    [Route("datafactory")]
    public async Task<ActionResult> GetDataFactories()
    {
        try
        {
            var factories = await _configService.GetAllDataFactoriesAsync();
            if (factories.Count == 0)
                return NotFound();

            return Ok(factories);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Gets all available Data Factories that can be created or executed in Azure
    /// </summary>
    /// <param name="factoryId">Database identifier of the Data Factory associated with the pipeline</param>
    /// <returns>
    ///     List of data factories
    /// </returns>
    [ProducesResponseType(typeof(List<DataFactory>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundObjectResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    [HttpGet]
    [Route("datafactory/{factoryId}")]
    public async Task<ActionResult> GetDataFactoryPipelines(int factoryId)
    {
        try
        {
            var pipelines = await _configService.GetAllDataFactoryPipelinesAsync(factoryId);
            if (pipelines.Count == 0)
                return NotFound();

            return Ok(pipelines);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Checks the status of a Pipeline run based on the <code>runid</code> returned from the initial Pipeline run request.
    /// </summary>
    /// <param name="factoryId">Database identifier of the Data Factory associated with the pipeline</param>
    /// <param name="runid">id returned from the initial Pipeline run request</param>
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    [HttpGet]
    [Route("datafactory/{factoryId}/pipeline/status/{runid}")]
    public async Task<ActionResult> GetPipelineStatus(int factoryId, string runid)
    {
        try
        {
            var status = await _adfAutomation.GetPipelineStatus(factoryId, runid);
            if (string.IsNullOrEmpty(status))
                return BadRequest("Pipeline run status check failed. View logs for more detail.");

            return Ok(status);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Creates a new DataFactory instance based on the one defined in the database which is
    ///     associated with the <code>factoryid</code>.
    /// </summary>
    /// <param name="factoryId">Database identifier of the DataFactory instance to create</param>
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    [HttpPost]
    [Route("datafactory/create/{factoryId}")]
    public async Task<ActionResult> Create(int factoryId)
    {
        try
        {
            var success = await _adfAutomation.CreateDataFactory(factoryId);
            if (!success) return BadRequest("Data Factory creation failed. View logs for more detail.");

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    ///     Triggers a Pipeline run based on the one defined in the database which is
    ///     associated with the <code>pipelineid</code>.
    /// </summary>
    /// <param name="factoryId">Database identifier of the Data Factory associated with the pipeline</param>
    /// <param name="pipelineid">Database identifier of the Pipeline instance to trigger</param>
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
    [HttpPost]
    [Route("datafactory/{factoryId}/pipeline/{pipelineid}/trigger")]
    public async Task<ActionResult> TriggerPipeline(int factoryId, int pipelineid)
    {
        try
        {
            var runID = await _adfAutomation.TriggerPipeline(factoryId, pipelineid);
            if (string.IsNullOrEmpty(runID)) return BadRequest("Pipeline trigger failed. View logs for more detail.");

            return Ok(runID);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}