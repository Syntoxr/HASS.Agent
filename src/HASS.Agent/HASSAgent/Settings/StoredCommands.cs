﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HASSAgent.Enums;
using HASSAgent.Models.Config;
using HASSAgent.Models.HomeAssistant.Commands;
using HASSAgent.Models.HomeAssistant.Commands.CustomCommands;
using HASSAgent.Models.HomeAssistant.Commands.KeyCommands;
using Newtonsoft.Json;
using Serilog;

namespace HASSAgent.Settings
{
    /// <summary>
    /// Handles loading and storing commands
    /// </summary>
    internal static class StoredCommands
    {
        /// <summary>
        /// Load all stored commands
        /// </summary>
        /// <returns></returns>
        internal static bool Load()
        {
            try
            {
                // add an empty list
                Variables.Commands = new List<AbstractCommand>();

                // check for existing file
                if (!File.Exists(Variables.CommandsFile))
                {
                    // none yet
                    Log.Information("[SETTINGS_COMMANDS] Config not found, no entities loaded");
                    Variables.MainForm?.SetCommandsStatus(ComponentStatus.Stopped);
                    return true;
                }

                // read the content
                var commandsRaw = File.ReadAllText(Variables.CommandsFile);
                if (string.IsNullOrWhiteSpace(commandsRaw))
                {
                    Log.Information("[SETTINGS_COMMANDS] Config is empty, no entities loaded");
                    Variables.MainForm?.SetCommandsStatus(ComponentStatus.Stopped);
                    return true;
                }

                // deserialize
                var configuredCommands = JsonConvert.DeserializeObject<List<ConfiguredCommand>>(commandsRaw);

                // null-check
                if (configuredCommands == null)
                {
                    Log.Error("[SETTINGS_COMMANDS] Error loading entites: returned null object");
                    Variables.MainForm?.SetCommandsStatus(ComponentStatus.Failed);
                    return false;
                }

                // convert to abstract commands
                foreach (var abstractCommand in configuredCommands.Select(ConvertConfiguredToAbstract).Where(abstractCommand => abstractCommand != null))
                {
                    Variables.Commands.Add(abstractCommand);
                }

                // all good
                Log.Information("[SETTINGS_COMMANDS] Loaded {count} entities", Variables.Commands.Count);
                Variables.MainForm?.SetCommandsStatus(ComponentStatus.Ok);
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[SETTINGS_COMMANDS] Error loading entities: {err}", ex.Message);
                Variables.MainForm?.ShowMessageBox($"Error loading commands:\r\n\r\n{ex.Message}", true);

                Variables.MainForm?.SetCommandsStatus(ComponentStatus.Failed);
                return false;
            }
        }

        /// <summary>
        /// Convert a 'ConfiguredCommand' (local storage, UI) to an 'AbstractCommand' (MQTT)
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        internal static AbstractCommand ConvertConfiguredToAbstract(ConfiguredCommand command)
        {
            AbstractCommand abstractCommand = null;

            switch (command.Type)
            {
                case CommandType.ShutdownCommand:
                    abstractCommand = new ShutdownCommand(command.Name, command.Id.ToString());
                    break;
                case CommandType.RestartCommand:
                    abstractCommand = new RestartCommand(command.Name, command.Id.ToString());
                    break;
                case CommandType.LogOffCommand:
                    abstractCommand = new LogOffCommand(command.Name, command.Id.ToString());
                    break;
                case CommandType.LockCommand:
                    abstractCommand = new LockCommand(command.Name, command.Id.ToString());
                    break;
                case CommandType.CustomCommand:
                    abstractCommand = new CustomCommand(command.Command, command.Name, command.Id.ToString());
                    break;
                case CommandType.MediaPlayPauseCommand:
                    abstractCommand = new MediaPlayPauseCommand(command.Name, command.Id.ToString());
                    break;
                case CommandType.MediaNextCommand:
                    abstractCommand = new MediaNextCommand(command.Name, command.Id.ToString());
                    break;
                case CommandType.MediaPreviousCommand:
                    abstractCommand = new MediaPreviousCommand(command.Name, command.Id.ToString());
                    break;
                case CommandType.MediaVolumeUpCommand:
                    abstractCommand = new MediaVolumeUpCommand(command.Name, command.Id.ToString());
                    break;
                case CommandType.MediaVolumeDownCommand:
                    abstractCommand = new MediaVolumeDownCommand(command.Name, command.Id.ToString());
                    break;
                case CommandType.MediaMuteCommand:
                    abstractCommand = new MediaMuteCommand(command.Name, command.Id.ToString());
                    break;
                case CommandType.KeyCommand:
                    abstractCommand = new KeyCommand(command.KeyCode, command.Name, command.Id.ToString());
                    break;
                default:
                    Log.Error("[SETTINGS_COMMANDS] [{name}] Unknown configured command type: {type}", command.Name, command.Type.ToString());
                    break;
            }

            return abstractCommand;
        }

        /// <summary>
        /// Convert an 'AbstractCommand' (MQTT) to an 'ConfiguredCommand' (local storage, UI)
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        internal static ConfiguredCommand ConvertAbstractToConfigured(AbstractCommand command)
        {
            switch (command)
            {
                case CustomCommand customCommand:
                {
                    _ = Enum.TryParse<CommandType>(customCommand.GetType().Name, out var type);
                    return new ConfiguredCommand()
                    {
                        Id = Guid.Parse(customCommand.Id), 
                        Name = customCommand.Name, 
                        Type = type, 
                        Command = customCommand.Command
                    };
                }

                case KeyCommand customKeyCommand:
                {
                    _ = Enum.TryParse<CommandType>(customKeyCommand.GetType().Name, out var type);
                    return new ConfiguredCommand()
                    {
                        Id = Guid.Parse(customKeyCommand.Id), 
                        Name = customKeyCommand.Name, 
                        Type = type,
                        KeyCode = customKeyCommand.KeyCode
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Store all current commands
        /// </summary>
        /// <returns></returns>
        internal static bool Store()
        {
            try
            {
                // convert commands
                var configuredCommands = Variables.Commands.Select(ConvertAbstractToConfigured).Where(configuredCommand => configuredCommand != null).ToList();

                // serialize to file
                var commands = JsonConvert.SerializeObject(configuredCommands, Formatting.Indented);
                File.WriteAllText(Variables.CommandsFile, commands);

                // done
                Log.Information("[SETTINGS_COMMANDS] Stored {count} entities", Variables.Commands.Count);
                Variables.MainForm?.SetCommandsStatus(ComponentStatus.Ok);
                return true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "[SETTINGS_COMMANDS] Error storing entities: {err}", ex.Message);
                Variables.MainForm?.ShowMessageBox($"Error storing commands:\r\n\r\n{ex.Message}", true);

                Variables.MainForm?.SetCommandsStatus(ComponentStatus.Failed);
                return false;
            }
        }
    }
}
