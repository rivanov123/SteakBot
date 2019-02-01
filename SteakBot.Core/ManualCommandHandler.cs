﻿using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SteakBot.Core
{
	internal enum ResultType
	{
		Text = 1,
		Image = 2
	}

	internal class MemeCommand
	{
		public ResultType ResultType { get; set; }

		public string Name { get; set; }

		public string Value { get; set; }

		public string Description { get; set; }

		public MemeCommand(ResultType resultType, string name, string value, string description)
		{
			ResultType = resultType;
			Name = name;
			Value = value;
			Description = description;
		}
	}

	internal static class ManualCommandHandler
	{
		private static readonly string MemeCommandsFileName = ConfigurationManager.AppSettings["memeCommandsRelativeFilePath"];
		private static Lazy<IList<MemeCommand>> _commands = new Lazy<IList<MemeCommand>>(() => LoadCommands());
		
		internal static IList<MemeCommand> Commands { get { return _commands.Value; } }

		public static async Task<bool> HandleCommandAsync(SocketUserMessage message)
		{
			var channel = message.Channel;
			var messageString = message.Content;

			if (!messageString.StartsWith("!"))
			{
				return false;
			}

			var commandText = messageString.Substring(1);

			if (messageString.EndsWith("!"))
			{
				commandText = commandText.Substring(0, commandText.Length - 1);
				await channel.DeleteMessagesAsync(new[] { message }, RequestOptions.Default);
			}

			var command = Commands.FirstOrDefault(x => x.Name == commandText);
			if (command == null)
			{
				return false;
			}

			switch (command.ResultType)
			{
				case ResultType.Text:
				{
					await channel.SendMessageAsync(command.Value);
					return true;
				}

				case ResultType.Image:
				{
					var embed = new EmbedBuilder
					{
						ImageUrl = command.Value
					};

					await channel.SendMessageAsync("", embed: embed);
					return true;
				}

				default:
				{
					return false;
				}
			}
		}

		private static void SaveCommands()
		{
			using (StreamWriter fileWriter = new StreamWriter(MemeCommandsFileName))
			{
				JsonSerializer jsonSerializer = new JsonSerializer();
				jsonSerializer.Serialize(fileWriter, Commands);
			}
		}

		private static IList<MemeCommand> LoadCommands()
		{
			using (StreamReader fileReader = new StreamReader(MemeCommandsFileName))
			{
				using (JsonReader jsonTextReader = new JsonTextReader(fileReader))
				{
					JsonSerializer jsonSerializer = new JsonSerializer();
					return jsonSerializer.Deserialize<IList<MemeCommand>>(jsonTextReader);
				}
			}
		}
	}
}
