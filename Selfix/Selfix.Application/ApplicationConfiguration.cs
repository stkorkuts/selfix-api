using Microsoft.Extensions.DependencyInjection;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.UseCases.Jobs.AvatarCreation;
using Selfix.Application.UseCases.Jobs.ImageGeneration;
using Selfix.Application.UseCases.Jobs.PromptProcessing;
using Selfix.Application.UseCases.Notifications.ShowTrialPackage;
using Selfix.Application.UseCases.Orders.Cancel;
using Selfix.Application.UseCases.Orders.Confirm;
using Selfix.Application.UseCases.Orders.Create;
using Selfix.Application.UseCases.Statistics.Application;
using Selfix.Application.UseCases.Telegram.Avatars.Creation.AddImage;
using Selfix.Application.UseCases.Telegram.Avatars.Creation.Cancel;
using Selfix.Application.UseCases.Telegram.Avatars.Creation.Confirm;
using Selfix.Application.UseCases.Telegram.Avatars.Creation.SetName;
using Selfix.Application.UseCases.Telegram.Avatars.GetAvatars;
using Selfix.Application.UseCases.Telegram.Avatars.SetActive;
using Selfix.Application.UseCases.Telegram.Images.GenerateByPredefinedPrompt;
using Selfix.Application.UseCases.Telegram.Products.Get;
using Selfix.Application.UseCases.Telegram.Profiles.Create;
using Selfix.Application.UseCases.Telegram.Profiles.Get;
using Selfix.Application.UseCases.Telegram.Profiles.Update.Settings;
using Selfix.Application.UseCases.Telegram.Profiles.Update.State;
using Selfix.Application.UseCases.Telegram.Promocodes.TryApplyPromocode;
using Selfix.Application.UseCases.Telegram.Promocodes.ValidatePromocode;
using Selfix.Application.UseCases.Telegram.Prompts.Get;
using Selfix.Application.UseCases.Telegram.Prompts.ProcessRawUserPrompt;
using Selfix.Application.UseCases.Telegram.Prompts.ValidateCustomPrompt;

namespace Selfix.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services.AddCachedRepositories().AddUseCases();
    }
    
    private static IServiceCollection AddCachedRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<CachedPromptsRepository>()
            .AddScoped<CachedTelegramProfilesRepository>()
            .AddScoped<CachedUsersRepository>();
    }

    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        return services
            .AddScoped<IHandleAvatarCreationResultUseCase, HandleAvatarCreationResultUseCase>()
            .AddScoped<IHandleImageGenerationResultUseCase, HandleImageGenerationResultUseCase>()
            .AddScoped<IShowTrialPackageNotificationUseCase, ShowTrialPackageNotificationUseCase>()
            .AddScoped<ICancelOrderUseCase, CancelOrderUseCase>()
            .AddScoped<IConfirmOrderUseCase, ConfirmOrderUseCase>()
            .AddScoped<ICreateOrderUseCase, CreateOrderUseCase>()
            .AddScoped<IAvatarCreationAddImageUseCase, AvatarCreationAddImageUseCase>()
            .AddScoped<IAvatarCreationCancelUseCase, AvatarCreationCancelUseCase>()
            .AddScoped<IAvatarCreationConfirmUseCase, AvatarCreationConfirmUseCase>()
            .AddScoped<IAvatarCreationSetAvatarNameUseCase, AvatarCreationSetAvatarNameUseCase>()
            .AddScoped<IGetAvatarsUseCase, GetAvatarsUseCase>()
            .AddScoped<ISetActiveAvatarUseCase, SetActiveAvatarUseCase>()
            .AddScoped<IProcessRawUserPromptUseCase, ProcessRawUserPromptUseCase>()
            .AddScoped<IProcessPredefinedPromptUseCase, ProcessPredefinedPromptUseCase>()
            .AddScoped<IGetProductsUseCase, GetProductsUseCase>()
            .AddScoped<ICreateIfNotCreatedTelegramProfileUseCase, CreateIfNotCreatedTelegramProfileUseCase>()
            .AddScoped<IGetTelegramProfileWithUserUseCase, GetTelegramProfileWithUserUseCase>()
            .AddScoped<IUpdateTelegramProfileSettingsUseCase, UpdateTelegramProfileSettingsUseCase>()
            .AddScoped<IUpdateTelegramProfileStateUseCase, UpdateTelegramProfileStateUseCase>()
            .AddScoped<ITryApplyPromocodeUseCase, TryApplyPromocodeUseCase>()
            .AddScoped<IValidatePromocodeUseCase, ValidatePromocodeUseCase>()
            .AddScoped<IGetPromptsUseCase, GetPromptsUseCase>()
            .AddScoped<IValidateCustomPromptUseCase, ValidateCustomPromptUseCase>()
            .AddScoped<IHandlePromptProcessingResultUseCase, HandlePromptProcessingResultUseCase>()
            .AddScoped<IGetApplicationStatisticsUseCase, GetApplicationStatisticsUseCase>()
            .AddScoped<IGetOrderUseCase, GetOrderUseCase>();
    }
}