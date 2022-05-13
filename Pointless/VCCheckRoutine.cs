using Discord.WebSocket;
using Pointless.Managements;

namespace Pointless
{
    public class VCCheckRoutine
    {
        public static async Task Start()
        {
            while (true)
            {
                await Task.Delay(1000 * 60 * int.Parse(Configs.Get("VC_CHECK_ROUTINE_DELAY_CYCLE")));

                try
                {
                    foreach (SocketGuild guild in Program.Client.Guilds)
                    {
                        foreach (SocketVoiceChannel vc in guild.VoiceChannels)
                        {
                            foreach (SocketGuildUser user in vc.Users)
                            {
                                if (user.IsBot)
                                {
                                    continue;
                                }

                                float amount = float.Parse(Configs.Get("DEFAULT_VC_POINT"));

                                if (!user.IsSelfMuted && !user.IsMuted)
                                {
                                    amount += float.Parse(Configs.Get("MIC_ON"));
                                }
                                if (user.IsVideoing)
                                {
                                    amount += float.Parse(Configs.Get("VIDEO_ON"));
                                }
                                if (user.IsStreaming)
                                {
                                    amount += float.Parse(Configs.Get("STREAM_ON"));
                                }

                                Points.AddFloatPoint(guild.Id, user.Id, amount);
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}
