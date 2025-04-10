using RazorLight;

public class RazorViewRenderService
{
    private readonly RazorLightEngine _engine;

    public RazorViewRenderService()
    {
        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates"))
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
    {
        string viewPath = $"{templateName}.cshtml";
        return await _engine.CompileRenderAsync(viewPath, model);
    }
}