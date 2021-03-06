using System.Windows.Controls;

namespace SwtorCaster.ViewModels
{
    using MahApps.Metro.Controls;
    using SwtorCaster.Core.Services.Settings;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Caliburn.Micro;
    using Core.Domain.Log;
    using Core.Domain.Settings;
    using Core.Services.Audio;
    using MahApps.Metro.Controls.Dialogs;
    using Microsoft.Win32;

    public class EventSettingItem : Screen
    {
        private readonly EventSettingsViewModel _eventSettingsViewModel;
        private readonly IAudioService _audioService;
        private readonly ISettingsService _settingsService;

        public EventSetting EventSetting { get; }

        public IEnumerable<SoundEvent> EffectNames => new[]
        {
            SoundEvent.EnterCombat,
            SoundEvent.ExitCombat,
            SoundEvent.AbilityActivate,
            SoundEvent.AbilityCancel,
            SoundEvent.Kill, 
            SoundEvent.Death,
            SoundEvent.Revived
        };

        public EventSettingItem(EventSettingsViewModel eventSettingsViewModel, EventSetting eventSetting, IAudioService audioService, ISettingsService settingsService)
        {
            _eventSettingsViewModel = eventSettingsViewModel;
            EventSetting = eventSetting;
            _audioService = audioService;
            _settingsService = settingsService;
        }

        public string AbilityId
        {
            get { return EventSetting.AbilityId; }
            set { EventSetting.AbilityId = value; }
        }

        public bool Enabled
        {
            get { return EventSetting.Enabled; }
            set { EventSetting.Enabled = value; }
        }

        public SoundEvent SelectedEffectName
        {
            get { return EventSetting.EffectName; }
            set { EventSetting.EffectName = value; }
        }

        public string Sound
        {
            get { return Path.GetFileNameWithoutExtension(EventSetting.Sound); }
            set
            {
                EventSetting.Sound = value;
                NotifyOfPropertyChange(() => Sound);
            }
        }

        public void AddAudio()
        {
            FileDialog fileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Audio files (mp3,wav,wma)|*.mp3;*.wav;*wma",
                Title = "Select audio to play",
                CheckPathExists = true,
                CheckFileExists = true
            };

            var result = fileDialog.ShowDialog();

            if (result.GetValueOrDefault())
            {
                Sound = fileDialog.FileName;
            }
        }

        public async void Play()
        {
            try
            {
                _audioService.Play(EventSetting.Sound, _settingsService.Settings.Volume);
            }
            catch (Exception e)
            {
                await (GetView() as MetroWindow).ShowMessageAsync("Error playing sound", e.Message);
            }
        }

        public async void Stop()
        {
            try
            {
                _audioService.Stop();
            }
            catch(Exception e)
            {
                await _eventSettingsViewModel.Window.ShowMessageAsync("Error stopping sound", e.Message);
            }
        }

        public async void Delete()
        {
            try
            {
                var result = await _eventSettingsViewModel.Window.ShowMessageAsync("Delete sound setting", "Are you sure?", MessageDialogStyle.AffirmativeAndNegative);

                if (result == MessageDialogResult.Affirmative)
                {
                    _eventSettingsViewModel.EventSettingViewModels.Remove(this);
                }
            }
            catch (Exception e)
            {
                await _eventSettingsViewModel.Window.ShowMessageAsync("Error deleting sound", e.Message);
            }
        }
    }
}