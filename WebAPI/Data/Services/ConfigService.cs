using Microsoft.EntityFrameworkCore;
using WebAPI.Data.Context;
using WebAPI.Data.Database;

namespace WebAPI.Data.Services;

public class ConfigService
{
    private readonly ConfigContext _configContext;

    public ConfigService(ConfigContext configContext)
    {
        _configContext = configContext;
    }

    public async Task<List<DataFactory>> GetAllDataFactoriesAsync()
    {
        return await _configContext.DataFactories.AsNoTracking().ToListAsync();
    }

    public async Task<List<Pipeline>> GetAllDataFactoryPipelinesAsync(int dataFactoryId)
    {
        return await _configContext.Pipelines.AsNoTracking()
            .Where(p => p.DataFactoryId == dataFactoryId).ToListAsync();
    }

    public async Task<DataFactory?> GetDataFactoryByIdAsync(int id)
    {
        return await _configContext.DataFactories.AsNoTracking().Where(x => x.DataFactoryId == id)
            .Include(x => x.LinkedServices)
            .ThenInclude(x => x.AzureSqlConfig)
            .Include(x => x.LinkedServices)
            .ThenInclude(x => x.SalesforceConfig)
            .Include(x => x.Datasets)
            .ThenInclude(x => x.AzureSqlLinkedService)
            .Include(x => x.Datasets)
            .ThenInclude(x => x.SalesforceLinkedService)
            .Include(x => x.Pipelines)
            .ThenInclude(x => x.CopyActivities)
            .ThenInclude(x => x.Source)
            .ThenInclude(x => x.Dataset)
            .Include(x => x.Pipelines)
            .ThenInclude(x => x.CopyActivities)
            .ThenInclude(x => x.Sink)
            .ThenInclude(x => x.Dataset)
            .FirstOrDefaultAsync();
    }

    public async Task<Pipeline?> GePipelineByIdAsync(int id)
    {
        return await _configContext.Pipelines.AsNoTracking().FirstOrDefaultAsync(x => x.PipelineId == id);
    }
}