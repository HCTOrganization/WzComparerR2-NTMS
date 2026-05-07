using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WzComparerR2
{
    public class DiscordService
    {
        private readonly DiscordSocketClient _client;
        private readonly string _botToken;
        private readonly List<string> _channelIds;

        public DiscordService()
        {
            _botToken = BotToken;
            _channelIds = ChannelID();
            _client = new DiscordSocketClient();
        }

        public async Task InitializeAsync()
        {
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;

            await _client.LoginAsync(TokenType.Bot, _botToken);
            await _client.StartAsync();
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            // Console.WriteLine($"{_client.CurrentUser} is connected!");
        }

        /// <summary>
        /// Send a message to a specific channel by index.
        /// </summary>
        public async Task SendMessageAsync(int channelIndex, string message)
        {
            if (!IsDiscordConfigured()) return;

            if (channelIndex < 0 || channelIndex >= _channelIds.Count)
                throw new ArgumentOutOfRangeException(nameof(channelIndex).Trim(), "Invalid channel index.");

            if (ulong.TryParse(_channelIds[channelIndex], out ulong targetChannel))
            {
                var channel = await _client.GetChannelAsync(targetChannel) as IMessageChannel;
                if (channel != null)
                {
                    await channel.SendMessageAsync(message);
                }
            }
        }

        /// <summary>
        /// Broadcast a message to all channels in the list.
        /// </summary>
        public async Task BroadcastMessageAsync(string message)
        {
            if (!IsDiscordConfigured()) return;

            foreach (var id in _channelIds)
            {
                if (ulong.TryParse(id, out ulong targetChannel))
                {
                    var channel = await _client.GetChannelAsync(targetChannel) as IMessageChannel;
                    if (channel != null)
                    {
                        await channel.SendMessageAsync(message);
                    }
                }
            }
        }

        /// <summary>
        /// Send an image to a specific channel by index.
        /// </summary>
        public async Task SendImageAsync(int channelIndex, string filePath, string caption = null)
        {
            if (!IsDiscordConfigured()) return;

            if (channelIndex < 0 || channelIndex >= _channelIds.Count)
                throw new ArgumentOutOfRangeException(nameof(channelIndex), "Invalid channel index.");

            if (ulong.TryParse(_channelIds[channelIndex], out ulong targetChannel))
            {
                var channel = await _client.GetChannelAsync(targetChannel) as IMessageChannel;
                if (channel != null && File.Exists(filePath))
                    await channel.SendFileAsync(filePath, caption ?? string.Empty);

            }
        }

        /// <summary>
        /// Broadcast an image to all channels in the list.
        /// </summary>
        public async Task BroadcastImageAsync(string filePath, string caption = null)
        {
            if (!IsDiscordConfigured()) return;

            foreach (var id in _channelIds)
            {
                if (ulong.TryParse(id, out ulong targetChannel))
                {
                    var channel = await _client.GetChannelAsync(targetChannel) as IMessageChannel;
                    if (channel != null && File.Exists(filePath))
                    {
                        await channel.SendFileAsync(filePath, caption ?? string.Empty);
                    }
                }
            }
        }

        private List<string> ChannelID()
        {
            if (string.IsNullOrEmpty(ChannelIDList))
            {
                return new List<string>();
            }
            return ChannelIDList.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private bool IsDiscordConfigured()
        {
            return !string.IsNullOrEmpty(BotToken) && ChannelID().Count > 0;
        }

        #region Global Settings
        public static string BotToken { get; set; }

        public static string ChannelIDList { get; set; }
        #endregion
    }
}
