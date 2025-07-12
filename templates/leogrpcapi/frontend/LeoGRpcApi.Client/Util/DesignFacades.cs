using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using LeoGRpcApi.Client.Core.Services;
using LeoGRpcApi.Client.ViewModels;
using LeoGRpcApi.Shared.ApiContract;

namespace LeoGRpcApi.Client.Util;

/// <summary>
///     Provides dummy implementations of various services for design-time use
/// </summary>
public static class DesignFacades
{
    public sealed class DummyLogger<T> : ILogger<T>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                                Func<TState, Exception?, string> formatter) { }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }

    public sealed class DummyNinjaService : INinjaService
    {
        public ValueTask<IReadOnlyCollection<int>> GetAllNinjaIdsAsync() =>
            ValueTask.FromResult<IReadOnlyCollection<int>>([1, 2]);

        public ValueTask<NinjaDto> GetNinjaByIdAsync(int id) =>
            ValueTask.FromResult(new NinjaDto
            {
                Id = id,
                CodeName = $"Ninja{id}",
                Rank = NinjaRank.Genin,
                SpecialSkills = { "test" },
                WeaponProficiencies = { NinjaWeapon.Shuriken }
            });
    }

    public sealed class DummyMissionService : IMissionService
    {
        public ValueTask<IReadOnlyCollection<MissionDto>> GetAllMissionsAsync() =>
            ValueTask.FromResult<IReadOnlyCollection<MissionDto>>([
                new MissionDto
                {
                    Id = 1L,
                    Dangerousness = 0.025D,
                    Title = "Grocery Shopping",
                    Description = "Buy groceries for the week"
                },
                new MissionDto
                {
                    Id = 2L,
                    Dangerousness = 0.6D,
                    Title = "Rescue Operation"
                }
            ]);

        public ValueTask UpdateMissionAsync(long missionId, double dangerousness, string? description) =>
            throw new NotImplementedException();

        public ValueTask<bool> DeleteMissionAsync(long missionId) => throw new NotImplementedException();

        public ValueTask<MissionAssignmentResult> AssignNinjaToMissionAsync(long missionId, int ninjaId) =>
            throw new NotImplementedException();
    }

    public sealed class DummyToastService : IToastService
    {
        public void ShowMessage(string message, string title, NotificationType type)
        {
            Console.WriteLine($"Toast: [{title}] {message}");
        }

        public void SetWindow(Window window) { }
    }

    public sealed class DummyDialogService : IDialogService
    {
        public Task<TResult>
            ShowModalDialogAsync<TDialogWindow, TDialogViewModel, TResult>(
                Action<TDialogViewModel>? viewModelConfiguration = null) where TDialogWindow : Window, new()
                                                                         where TDialogViewModel :
                                                                         DialogViewModelBase<TResult> =>
            Task.FromResult(default(TResult)!);

        public void SetOwner(Window window) { }
    }

    public sealed class DummyMessenger : IMessenger
    {
        public bool IsRegistered<TMessage, TToken>(object recipient, TToken token)
            where TMessage : class where TToken : IEquatable<TToken> =>
            throw new NotImplementedException();

        public void Register<TRecipient, TMessage, TToken>(TRecipient recipient, TToken token,
                                                           MessageHandler<TRecipient, TMessage> handler)
            where TRecipient : class where TMessage : class where TToken : IEquatable<TToken>
        {
            throw new NotImplementedException();
        }

        public void UnregisterAll(object recipient)
        {
            throw new NotImplementedException();
        }

        public void UnregisterAll<TToken>(object recipient, TToken token) where TToken : IEquatable<TToken>
        {
            throw new NotImplementedException();
        }

        public void Unregister<TMessage, TToken>(object recipient, TToken token)
            where TMessage : class where TToken : IEquatable<TToken>
        {
            throw new NotImplementedException();
        }

        public TMessage Send<TMessage, TToken>(TMessage message, TToken token)
            where TMessage : class where TToken : IEquatable<TToken> =>
            throw new NotImplementedException();

        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
