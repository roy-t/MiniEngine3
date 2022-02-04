using System;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Controllers;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Entities;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;
using Mini.Engine.Graphics.Lighting.PointLights;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine;

[Service]
internal sealed class GameLoop : IGameLoop
{
    private readonly Device Device;

    private readonly RenderHelper Helper;
    private readonly FrameService FrameService;
    private readonly CameraController CameraController;
    private readonly ContentManager Content;
    private readonly ParallelPipeline Pipeline;    
    public GameLoop(Device device, RenderHelper helper, FrameService frameService, CameraController cameraController, RenderPipelineBuilder builder, ContentManager content, EntityAdministrator entities, ComponentAdministrator components, CubeMapGenerator cubeMapGenerator)
    {
        this.Device = device;
        this.Helper = helper;
        this.FrameService = frameService;
        this.CameraController = cameraController;
        this.Content = content;        

        content.Push("RenderPipeline");
        this.Pipeline = builder.Build();

        SetScene(content, entities, components, cubeMapGenerator);
        
    }

    private static void SetScene(ContentManager content, EntityAdministrator entities, ComponentAdministrator components, CubeMapGenerator cubeMapGenerator)
    {
        content.Push("Scene");
        // TODO: move to scene
        var entity = entities.Create();
        //components.Add(new ModelComponent(entity, content.LoadCube()));
        //components.Add(new TransformComponent(entity).SetScale(1.0f));
        components.Add(new ModelComponent(entity, content.LoadSponza()));
        components.Add(new TransformComponent(entity).SetScale(0.01f));
        //models.Add(new ModelComponent(entity, content.LoadAsteroid()));


        var sphere = entities.Create();
        //models.Add(new ModelComponent(sphere, SphereGenerator.Generate(device, 3, content.LoadDefaultMaterial(), "Sphere")));
        components.Add(new PointLightComponent(sphere, Vector4.One, 100.0f));
        components.Add(new TransformComponent(sphere));//.ApplyTranslation(Vector3.One * 10));



        var sky = entities.Create();
        //var texture = content.LoadTexture(@"Skyboxes\industrial.hdr");
        var texture = content.LoadTexture(@"Skyboxes\circus.hdr");
        var albedo = cubeMapGenerator.GenerateAlbedo(texture, false, "skybox_albedo");
        var irradiance = cubeMapGenerator.GenerateIrradiance(texture, "skybox_irradiance");
        var environment = cubeMapGenerator.GenerateEnvironment(texture, "skybox_environment");

        // Make sure the items are disposed whenever this content frame is
        content.Link(albedo, albedo.Name);
        content.Link(irradiance, irradiance.Name);
        content.Link(environment, environment.Name);

        components.Add(new SkyboxComponent(sky, albedo, irradiance, environment, 1.0f));
    }

    public void Update(float time, float elapsed)
    {
        this.Content.ReloadChangedContent();
        this.CameraController.Update(this.FrameService.Camera, elapsed);
    }

    public void Draw(float alpha)
    {
        this.FrameService.Alpha = alpha;
        this.Pipeline.Frame();

        this.Helper.RenderToViewPort(this.Device.ImmediateContext, this.FrameService.LBuffer.Light, 0, 0, this.Device.Width, this.Device.Height);
    }

    public void Dispose()
    {
        this.Pipeline.Dispose();
    }
}
